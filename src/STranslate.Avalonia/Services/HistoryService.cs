using Microsoft.Data.Sqlite;
using STranslate.Core;

namespace STranslate.Avalonia.Services;

/// <summary>一条翻译历史。</summary>
public sealed class HistoryItem
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public string SourceLang { get; set; } = string.Empty;
    public string TargetLang { get; set; } = string.Empty;
    public string SourceText { get; set; } = string.Empty;
    public string ResultText { get; set; } = string.Empty;
    public bool Favorite { get; set; }
    public string Note { get; set; } = string.Empty;
}

/// <summary>
/// 历史记录（SQLite，手写参数化 SQL）。
/// ponytail: 固定保留 1000 条（写入时裁剪），接入设置页容量配置时再参数化。
/// </summary>
public sealed class HistoryService
{
    private const int Capacity = 1000;

    private static readonly string DbPath =
        Path.Combine(DataLocation.DataDirectory(), "Avalonia", "history.db");

    private static string ConnectionString => $"Data Source={DbPath}";

    public HistoryService()
    {
        EnsureTable();
        MigrateAddNote();
    }

    private void EnsureTable()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Time TEXT NOT NULL,
                SourceLang TEXT,
                TargetLang TEXT,
                SourceText TEXT,
                ResultText TEXT,
                Favorite INTEGER NOT NULL DEFAULT 0
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private void MigrateAddNote()
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "ALTER TABLE History ADD COLUMN Note TEXT NOT NULL DEFAULT ''";
        try { cmd.ExecuteNonQuery(); }
        catch (SqliteException) { /* 列已存在 */ }
    }

    public void InsertOrUpdate(HistoryItem item)
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var tx = db.BeginTransaction();

        long? existing;
        using (var cmd = db.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = "SELECT Id FROM History WHERE SourceText = @t AND SourceLang = @sl AND TargetLang = @tl LIMIT 1";
            cmd.Parameters.AddWithValue("@t", item.SourceText);
            cmd.Parameters.AddWithValue("@sl", item.SourceLang);
            cmd.Parameters.AddWithValue("@tl", item.TargetLang);
            existing = cmd.ExecuteScalar() as long?;
        }

        using (var cmd = db.CreateCommand())
        {
            cmd.Transaction = tx;
            if (existing is { } id)
            {
                cmd.CommandText = "UPDATE History SET Time = @time, ResultText = @rt WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
            }
            else
            {
                cmd.CommandText = "INSERT INTO History (Time, SourceLang, TargetLang, SourceText, ResultText, Favorite) VALUES (@time, @sl, @tl, @t, @rt, 0)";
            }
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@sl", item.SourceLang);
            cmd.Parameters.AddWithValue("@tl", item.TargetLang);
            cmd.Parameters.AddWithValue("@t", item.SourceText);
            cmd.Parameters.AddWithValue("@rt", item.ResultText);
            cmd.ExecuteNonQuery();
        }

        // 容量裁剪：删除超出容量的最旧记录
        using (var cmd = db.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM History WHERE Id IN (SELECT Id FROM History WHERE Favorite = 0 ORDER BY Id DESC LIMIT -1 OFFSET @cap)";
            cmd.Parameters.AddWithValue("@cap", Capacity);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
    }

    /// <summary>按关键词搜索（源文/译文/备注），keyword 为空返回最近记录；favoriteOnly 过滤收藏。时间倒序。</summary>
    public List<HistoryItem> Search(string keyword, bool favoriteOnly = false, int limit = 200)
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        var where = favoriteOnly ? " WHERE Favorite = 1" : "";
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            where = (where.Length == 0 ? " WHERE " : " AND ") +
                    "(SourceText LIKE @kw OR ResultText LIKE @kw OR Note LIKE @kw)";
            cmd.Parameters.AddWithValue("@kw", $"%{keyword.Trim()}%");
        }
        cmd.CommandText = $"""
            SELECT Id, Time, SourceLang, TargetLang, SourceText, ResultText, Favorite, Note
            FROM History{where} ORDER BY Time DESC LIMIT @limit
            """;
        cmd.Parameters.AddWithValue("@limit", limit);

        var list = new List<HistoryItem>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new HistoryItem
            {
                Id = reader.GetInt64(0),
                Time = DateTime.TryParse(reader.GetString(1), out var t) ? t : DateTime.MinValue,
                SourceLang = reader.IsDBNull(2) ? "" : reader.GetString(2),
                TargetLang = reader.IsDBNull(3) ? "" : reader.GetString(3),
                SourceText = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ResultText = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Favorite = !reader.IsDBNull(6) && reader.GetInt64(6) != 0,
                Note = reader.IsDBNull(7) ? "" : reader.GetString(7)
            });
        }
        return list;
    }

    public void SetFavorite(long id, bool favorite)
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE History SET Favorite = @fav WHERE Id = @id";
        cmd.Parameters.AddWithValue("@fav", favorite ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void SetNote(long id, string note)
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE History SET Note = @note WHERE Id = @id";
        cmd.Parameters.AddWithValue("@note", note);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(long id)
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "DELETE FROM History WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Clear()
    {
        using var db = new SqliteConnection(ConnectionString);
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "DELETE FROM History";
        cmd.ExecuteNonQuery();
    }
}
