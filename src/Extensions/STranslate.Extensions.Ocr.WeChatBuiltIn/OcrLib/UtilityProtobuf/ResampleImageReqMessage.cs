using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace UtilityProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ResampleImageReqMessage : IMessage<ResampleImageReqMessage>, IMessage, IEquatable<ResampleImageReqMessage>, IDeepCloneable<ResampleImageReqMessage>, IBufferMessage
{
	private static readonly MessageParser<ResampleImageReqMessage> _parser = new MessageParser<ResampleImageReqMessage>((Func<ResampleImageReqMessage>)(() => new ResampleImageReqMessage()));

	private UnknownFieldSet _unknownFields;

	public const int DecodePicFieldNumber = 1;

	private string decodePic_ = "";

	public const int EncodePicFieldNumber = 2;

	private string encodePic_ = "";

	public const int RePicXFieldNumber = 3;

	private int rePicX_;

	public const int RePicYFieldNumber = 4;

	private int rePicY_;

	public const int Unknown0FieldNumber = 5;

	private int unknown0_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ResampleImageReqMessage> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => UtilityProtobufReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string DecodePic
	{
		get
		{
			return decodePic_;
		}
		set
		{
			decodePic_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string EncodePic
	{
		get
		{
			return encodePic_;
		}
		set
		{
			encodePic_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RePicX
	{
		get
		{
			return rePicX_;
		}
		set
		{
			rePicX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RePicY
	{
		get
		{
			return rePicY_;
		}
		set
		{
			rePicY_ = value;
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
	public ResampleImageReqMessage()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResampleImageReqMessage(ResampleImageReqMessage other)
		: this()
	{
		decodePic_ = other.decodePic_;
		encodePic_ = other.encodePic_;
		rePicX_ = other.rePicX_;
		rePicY_ = other.rePicY_;
		unknown0_ = other.unknown0_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResampleImageReqMessage Clone()
	{
		return new ResampleImageReqMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ResampleImageReqMessage);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ResampleImageReqMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DecodePic != other.DecodePic)
		{
			return false;
		}
		if (EncodePic != other.EncodePic)
		{
			return false;
		}
		if (RePicX != other.RePicX)
		{
			return false;
		}
		if (RePicY != other.RePicY)
		{
			return false;
		}
		if (Unknown0 != other.Unknown0)
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
		if (DecodePic.Length != 0)
		{
			num ^= DecodePic.GetHashCode();
		}
		if (EncodePic.Length != 0)
		{
			num ^= EncodePic.GetHashCode();
		}
		if (RePicX != 0)
		{
			num ^= RePicX.GetHashCode();
		}
		if (RePicY != 0)
		{
			num ^= RePicY.GetHashCode();
		}
		if (Unknown0 != 0)
		{
			num ^= Unknown0.GetHashCode();
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
		if (DecodePic.Length != 0)
		{
			output.WriteRawTag((byte)10);
			output.WriteString(DecodePic);
		}
		if (EncodePic.Length != 0)
		{
			output.WriteRawTag((byte)18);
			output.WriteString(EncodePic);
		}
		if (RePicX != 0)
		{
			output.WriteRawTag((byte)24);
			output.WriteInt32(RePicX);
		}
		if (RePicY != 0)
		{
			output.WriteRawTag((byte)32);
			output.WriteInt32(RePicY);
		}
		if (Unknown0 != 0)
		{
			output.WriteRawTag((byte)40);
			output.WriteInt32(Unknown0);
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
		if (DecodePic.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DecodePic);
		}
		if (EncodePic.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EncodePic);
		}
		if (RePicX != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RePicX);
		}
		if (RePicY != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RePicY);
		}
		if (Unknown0 != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unknown0);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ResampleImageReqMessage other)
	{
		if (other != null)
		{
			if (other.DecodePic.Length != 0)
			{
				DecodePic = other.DecodePic;
			}
			if (other.EncodePic.Length != 0)
			{
				EncodePic = other.EncodePic;
			}
			if (other.RePicX != 0)
			{
				RePicX = other.RePicX;
			}
			if (other.RePicY != 0)
			{
				RePicY = other.RePicY;
			}
			if (other.Unknown0 != 0)
			{
				Unknown0 = other.Unknown0;
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
			case 10u:
				DecodePic = input.ReadString();
				break;
			case 18u:
				EncodePic = input.ReadString();
				break;
			case 24u:
				RePicX = input.ReadInt32();
				break;
			case 32u:
				RePicY = input.ReadInt32();
				break;
			case 40u:
				Unknown0 = input.ReadInt32();
				break;
			}
		}
	}
}
