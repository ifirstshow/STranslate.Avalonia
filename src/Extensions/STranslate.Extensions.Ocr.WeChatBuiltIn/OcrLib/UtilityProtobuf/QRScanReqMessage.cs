using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace UtilityProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class QRScanReqMessage : IMessage<QRScanReqMessage>, IMessage, IEquatable<QRScanReqMessage>, IDeepCloneable<QRScanReqMessage>, IBufferMessage
{
	private static readonly MessageParser<QRScanReqMessage> _parser = new MessageParser<QRScanReqMessage>((Func<QRScanReqMessage>)(() => new QRScanReqMessage()));

	private UnknownFieldSet _unknownFields;

	public const int OriginPicFieldNumber = 1;

	private string originPic_ = "";

	public const int DecodePicFieldNumber = 2;

	private string decodePic_ = "";

	public const int Unknown0FieldNumber = 3;

	private int unknown0_;

	public const int EncodePicDataFieldNumber = 4;

	private ByteString encodePicData_ = ByteString.Empty;

	public const int EncodePicXFieldNumber = 5;

	private int encodePicX_;

	public const int EncodePicYFieldNumber = 6;

	private int encodePicY_;

	public const int Unknown1FieldNumber = 7;

	private int unknown1_;

	public const int TextScanIdFieldNumber = 8;

	private int textScanId_;

	public const int Unknown3FieldNumber = 9;

	private int unknown3_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<QRScanReqMessage> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => UtilityProtobufReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string OriginPic
	{
		get
		{
			return originPic_;
		}
		set
		{
			originPic_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

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
	public ByteString EncodePicData
	{
		get
		{
			return encodePicData_;
		}
		set
		{
			encodePicData_ = ProtoPreconditions.CheckNotNull<ByteString>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EncodePicX
	{
		get
		{
			return encodePicX_;
		}
		set
		{
			encodePicX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EncodePicY
	{
		get
		{
			return encodePicY_;
		}
		set
		{
			encodePicY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Unknown1
	{
		get
		{
			return unknown1_;
		}
		set
		{
			unknown1_ = value;
		}
	}

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
	public int Unknown3
	{
		get
		{
			return unknown3_;
		}
		set
		{
			unknown3_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRScanReqMessage()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRScanReqMessage(QRScanReqMessage other)
		: this()
	{
		originPic_ = other.originPic_;
		decodePic_ = other.decodePic_;
		unknown0_ = other.unknown0_;
		encodePicData_ = other.encodePicData_;
		encodePicX_ = other.encodePicX_;
		encodePicY_ = other.encodePicY_;
		unknown1_ = other.unknown1_;
		textScanId_ = other.textScanId_;
		unknown3_ = other.unknown3_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRScanReqMessage Clone()
	{
		return new QRScanReqMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as QRScanReqMessage);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(QRScanReqMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (OriginPic != other.OriginPic)
		{
			return false;
		}
		if (DecodePic != other.DecodePic)
		{
			return false;
		}
		if (Unknown0 != other.Unknown0)
		{
			return false;
		}
		if (EncodePicData != other.EncodePicData)
		{
			return false;
		}
		if (EncodePicX != other.EncodePicX)
		{
			return false;
		}
		if (EncodePicY != other.EncodePicY)
		{
			return false;
		}
		if (Unknown1 != other.Unknown1)
		{
			return false;
		}
		if (TextScanId != other.TextScanId)
		{
			return false;
		}
		if (Unknown3 != other.Unknown3)
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
		if (OriginPic.Length != 0)
		{
			num ^= OriginPic.GetHashCode();
		}
		if (DecodePic.Length != 0)
		{
			num ^= DecodePic.GetHashCode();
		}
		if (Unknown0 != 0)
		{
			num ^= Unknown0.GetHashCode();
		}
		if (EncodePicData.Length != 0)
		{
			num ^= ((object)EncodePicData).GetHashCode();
		}
		if (EncodePicX != 0)
		{
			num ^= EncodePicX.GetHashCode();
		}
		if (EncodePicY != 0)
		{
			num ^= EncodePicY.GetHashCode();
		}
		if (Unknown1 != 0)
		{
			num ^= Unknown1.GetHashCode();
		}
		if (TextScanId != 0)
		{
			num ^= TextScanId.GetHashCode();
		}
		if (Unknown3 != 0)
		{
			num ^= Unknown3.GetHashCode();
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
		if (OriginPic.Length != 0)
		{
			output.WriteRawTag((byte)10);
			output.WriteString(OriginPic);
		}
		if (DecodePic.Length != 0)
		{
			output.WriteRawTag((byte)18);
			output.WriteString(DecodePic);
		}
		if (Unknown0 != 0)
		{
			output.WriteRawTag((byte)24);
			output.WriteInt32(Unknown0);
		}
		if (EncodePicData.Length != 0)
		{
			output.WriteRawTag((byte)34);
			output.WriteBytes(EncodePicData);
		}
		if (EncodePicX != 0)
		{
			output.WriteRawTag((byte)40);
			output.WriteInt32(EncodePicX);
		}
		if (EncodePicY != 0)
		{
			output.WriteRawTag((byte)48);
			output.WriteInt32(EncodePicY);
		}
		if (Unknown1 != 0)
		{
			output.WriteRawTag((byte)56);
			output.WriteInt32(Unknown1);
		}
		if (TextScanId != 0)
		{
			output.WriteRawTag((byte)64);
			output.WriteInt32(TextScanId);
		}
		if (Unknown3 != 0)
		{
			output.WriteRawTag((byte)72);
			output.WriteInt32(Unknown3);
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
		if (OriginPic.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OriginPic);
		}
		if (DecodePic.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DecodePic);
		}
		if (Unknown0 != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unknown0);
		}
		if (EncodePicData.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(EncodePicData);
		}
		if (EncodePicX != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EncodePicX);
		}
		if (EncodePicY != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EncodePicY);
		}
		if (Unknown1 != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unknown1);
		}
		if (TextScanId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TextScanId);
		}
		if (Unknown3 != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unknown3);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(QRScanReqMessage other)
	{
		if (other != null)
		{
			if (other.OriginPic.Length != 0)
			{
				OriginPic = other.OriginPic;
			}
			if (other.DecodePic.Length != 0)
			{
				DecodePic = other.DecodePic;
			}
			if (other.Unknown0 != 0)
			{
				Unknown0 = other.Unknown0;
			}
			if (other.EncodePicData.Length != 0)
			{
				EncodePicData = other.EncodePicData;
			}
			if (other.EncodePicX != 0)
			{
				EncodePicX = other.EncodePicX;
			}
			if (other.EncodePicY != 0)
			{
				EncodePicY = other.EncodePicY;
			}
			if (other.Unknown1 != 0)
			{
				Unknown1 = other.Unknown1;
			}
			if (other.TextScanId != 0)
			{
				TextScanId = other.TextScanId;
			}
			if (other.Unknown3 != 0)
			{
				Unknown3 = other.Unknown3;
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
				OriginPic = input.ReadString();
				break;
			case 18u:
				DecodePic = input.ReadString();
				break;
			case 24u:
				Unknown0 = input.ReadInt32();
				break;
			case 34u:
				EncodePicData = input.ReadBytes();
				break;
			case 40u:
				EncodePicX = input.ReadInt32();
				break;
			case 48u:
				EncodePicY = input.ReadInt32();
				break;
			case 56u:
				Unknown1 = input.ReadInt32();
				break;
			case 64u:
				TextScanId = input.ReadInt32();
				break;
			case 72u:
				Unknown3 = input.ReadInt32();
				break;
			}
		}
	}
}
