using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace OcrProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OcrRequest : IMessage<OcrRequest>, IMessage, IEquatable<OcrRequest>, IDeepCloneable<OcrRequest>, IBufferMessage
{
	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static class Types
	{
		[DebuggerDisplay("{ToString(),nq}")]
		public sealed class PicPaths : IMessage<PicPaths>, IMessage, IEquatable<PicPaths>, IDeepCloneable<PicPaths>, IBufferMessage
		{
			private static readonly MessageParser<PicPaths> _parser = new MessageParser<PicPaths>((Func<PicPaths>)(() => new PicPaths()));

			private UnknownFieldSet _unknownFields;

			public const int PicPathFieldNumber = 1;

			private static readonly FieldCodec<string> _repeated_picPath_codec = FieldCodec.ForString(10u);

			private readonly RepeatedField<string> picPath_ = new RepeatedField<string>();

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static MessageParser<PicPaths> Parser => _parser;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static MessageDescriptor Descriptor => OcrRequest.Descriptor.NestedTypes[0];

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			MessageDescriptor IMessage.Descriptor => Descriptor;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public RepeatedField<string> PicPath => picPath_;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public PicPaths()
			{
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public PicPaths(PicPaths other)
				: this()
			{
				picPath_ = other.picPath_.Clone();
				_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public PicPaths Clone()
			{
				return new PicPaths(this);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public override bool Equals(object other)
			{
				return Equals(other as PicPaths);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public bool Equals(PicPaths other)
			{
				if (other == null)
				{
					return false;
				}
				if (other == this)
				{
					return true;
				}
				if (!picPath_.Equals(other.picPath_))
				{
					return false;
				}
				return object.Equals(_unknownFields, other._unknownFields);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public override int GetHashCode()
			{
				int num = 1;
				num ^= ((object)picPath_).GetHashCode();
				if (_unknownFields != null)
				{
					num ^= ((object)_unknownFields).GetHashCode();
				}
				return num;
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public override string ToString()
			{
				return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public void WriteTo(CodedOutputStream output)
			{
				output.WriteRawMessage((IMessage)(object)this);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			void IBufferMessage.InternalWriteTo(ref WriteContext output)
			{
				picPath_.WriteTo(ref output, _repeated_picPath_codec);
				if (_unknownFields != null)
				{
					_unknownFields.WriteTo(ref output);
				}
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public int CalculateSize()
			{
				int num = 0;
				num += picPath_.CalculateSize(_repeated_picPath_codec);
				if (_unknownFields != null)
				{
					num += _unknownFields.CalculateSize();
				}
				return num;
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public void MergeFrom(PicPaths other)
			{
				if (other != null)
				{
					picPath_.Add((IEnumerable<string>)other.picPath_);
					_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
				}
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public void MergeFrom(CodedInputStream input)
			{
				input.ReadRawMessage((IMessage)(object)this);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			void IBufferMessage.InternalMergeFrom(ref ParseContext input)
			{
				uint num;
				while ((num = input.ReadTag()) != 0)
				{
					if (num != 10)
					{
						_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
					}
					else
					{
						picPath_.AddEntriesFrom(ref input, _repeated_picPath_codec);
					}
				}
			}
		}
	}

	private static readonly MessageParser<OcrRequest> _parser = new MessageParser<OcrRequest>((Func<OcrRequest>)(() => new OcrRequest()));

	private UnknownFieldSet _unknownFields;

	public const int UnknowFieldNumber = 1;

	private int unknow_;

	public const int TaskIdFieldNumber = 2;

	private int taskId_;

	public const int PicPathFieldNumber = 3;

	private Types.PicPaths picPath_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OcrRequest> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => OcrProtobufReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Unknow
	{
		get
		{
			return unknow_;
		}
		set
		{
			unknow_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TaskId
	{
		get
		{
			return taskId_;
		}
		set
		{
			taskId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Types.PicPaths PicPath
	{
		get
		{
			return picPath_;
		}
		set
		{
			picPath_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OcrRequest()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OcrRequest(OcrRequest other)
		: this()
	{
		unknow_ = other.unknow_;
		taskId_ = other.taskId_;
		picPath_ = ((other.picPath_ != null) ? other.picPath_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OcrRequest Clone()
	{
		return new OcrRequest(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OcrRequest);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OcrRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Unknow != other.Unknow)
		{
			return false;
		}
		if (TaskId != other.TaskId)
		{
			return false;
		}
		if (!object.Equals(PicPath, other.PicPath))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (Unknow != 0)
		{
			num ^= Unknow.GetHashCode();
		}
		if (TaskId != 0)
		{
			num ^= TaskId.GetHashCode();
		}
		if (picPath_ != null)
		{
			num ^= PicPath.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (Unknow != 0)
		{
			output.WriteRawTag((byte)8);
			output.WriteInt32(Unknow);
		}
		if (TaskId != 0)
		{
			output.WriteRawTag((byte)16);
			output.WriteInt32(TaskId);
		}
		if (picPath_ != null)
		{
			output.WriteRawTag((byte)26);
			output.WriteMessage((IMessage)(object)PicPath);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (Unknow != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unknow);
		}
		if (TaskId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TaskId);
		}
		if (picPath_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PicPath);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OcrRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Unknow != 0)
		{
			Unknow = other.Unknow;
		}
		if (other.TaskId != 0)
		{
			TaskId = other.TaskId;
		}
		if (other.picPath_ != null)
		{
			if (picPath_ == null)
			{
				PicPath = new Types.PicPaths();
			}
			PicPath.MergeFrom(other.PicPath);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				Unknow = input.ReadInt32();
				break;
			case 16u:
				TaskId = input.ReadInt32();
				break;
			case 26u:
				if (picPath_ == null)
				{
					PicPath = new Types.PicPaths();
				}
				input.ReadMessage((IMessage)(object)PicPath);
				break;
			}
		}
	}
}
