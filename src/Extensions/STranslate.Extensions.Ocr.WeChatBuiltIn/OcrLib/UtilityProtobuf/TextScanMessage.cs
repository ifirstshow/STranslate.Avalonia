using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace UtilityProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TextScanMessage : IMessage<TextScanMessage>, IMessage, IEquatable<TextScanMessage>, IDeepCloneable<TextScanMessage>, IBufferMessage
{
	private static readonly MessageParser<TextScanMessage> _parser = new MessageParser<TextScanMessage>((Func<TextScanMessage>)(() => new TextScanMessage()));

	private UnknownFieldSet _unknownFields;

	public const int TextScanIdFieldNumber = 1;

	private int textScanId_;

	public const int PicPathFieldNumber = 2;

	private string picPath_ = "";

	public const int HaveTextFieldNumber = 3;

	private int haveText_;

	public const int Unknown0FieldNumber = 4;

	private int unknown0_;

	public const int RateFieldNumber = 5;

	private float rate_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TextScanMessage> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => UtilityProtobufReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TextScanId
	{
		get
		{
			return textScanId_;
		}
		set
		{
			textScanId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string PicPath
	{
		get
		{
			return picPath_;
		}
		set
		{
			picPath_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HaveText
	{
		get
		{
			return haveText_;
		}
		set
		{
			haveText_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Unknown0
	{
		get
		{
			return unknown0_;
		}
		set
		{
			unknown0_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Rate
	{
		get
		{
			return rate_;
		}
		set
		{
			rate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TextScanMessage()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TextScanMessage(TextScanMessage other)
		: this()
	{
		textScanId_ = other.textScanId_;
		picPath_ = other.picPath_;
		haveText_ = other.haveText_;
		unknown0_ = other.unknown0_;
		rate_ = other.rate_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TextScanMessage Clone()
	{
		return new TextScanMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TextScanMessage);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TextScanMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TextScanId != other.TextScanId)
		{
			return false;
		}
		if (PicPath != other.PicPath)
		{
			return false;
		}
		if (HaveText != other.HaveText)
		{
			return false;
		}
		if (Unknown0 != other.Unknown0)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Rate, other.Rate))
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
		if (TextScanId != 0)
		{
			num ^= TextScanId.GetHashCode();
		}
		if (PicPath.Length != 0)
		{
			num ^= PicPath.GetHashCode();
		}
		if (HaveText != 0)
		{
			num ^= HaveText.GetHashCode();
		}
		if (Unknown0 != 0)
		{
			num ^= Unknown0.GetHashCode();
		}
		if (Rate != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Rate);
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
		if (TextScanId != 0)
		{
			output.WriteRawTag((byte)8);
			output.WriteInt32(TextScanId);
		}
		if (PicPath.Length != 0)
		{
			output.WriteRawTag((byte)18);
			output.WriteString(PicPath);
		}
		if (HaveText != 0)
		{
			output.WriteRawTag((byte)24);
			output.WriteInt32(HaveText);
		}
		if (Unknown0 != 0)
		{
			output.WriteRawTag((byte)32);
			output.WriteInt32(Unknown0);
		}
		if (Rate != 0f)
		{
			output.WriteRawTag((byte)45);
			output.WriteFloat(Rate);
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
		if (TextScanId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TextScanId);
		}
		if (PicPath.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PicPath);
		}
		if (HaveText != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HaveText);
		}
		if (Unknown0 != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unknown0);
		}
		if (Rate != 0f)
		{
			num += 5;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TextScanMessage other)
	{
		if (other != null)
		{
			if (other.TextScanId != 0)
			{
				TextScanId = other.TextScanId;
			}
			if (other.PicPath.Length != 0)
			{
				PicPath = other.PicPath;
			}
			if (other.HaveText != 0)
			{
				HaveText = other.HaveText;
			}
			if (other.Unknown0 != 0)
			{
				Unknown0 = other.Unknown0;
			}
			if (other.Rate != 0f)
			{
				Rate = other.Rate;
			}
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
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				TextScanId = input.ReadInt32();
				break;
			case 18u:
				PicPath = input.ReadString();
				break;
			case 24u:
				HaveText = input.ReadInt32();
				break;
			case 32u:
				Unknown0 = input.ReadInt32();
				break;
			case 45u:
				Rate = input.ReadFloat();
				break;
			}
		}
	}
}
