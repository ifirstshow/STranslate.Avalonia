using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace UtilityProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class QRScanRespMessage : IMessage<QRScanRespMessage>, IMessage, IEquatable<QRScanRespMessage>, IDeepCloneable<QRScanRespMessage>, IBufferMessage
{
	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static class Types
	{
		[DebuggerDisplay("{ToString(),nq}")]
		public sealed class QRScanResult : IMessage<QRScanResult>, IMessage, IEquatable<QRScanResult>, IDeepCloneable<QRScanResult>, IBufferMessage
		{
			private static readonly MessageParser<QRScanResult> _parser = new MessageParser<QRScanResult>((Func<QRScanResult>)(() => new QRScanResult()));

			private UnknownFieldSet _unknownFields;

			public const int ResultFieldNumber = 1;

			private ByteString result_ = ByteString.Empty;

			public const int Unknow0FieldNumber = 2;

			private int unknow0_;

			public const int Unknown1FieldNumber = 3;

			private double unknown1_;

			public const int Unknown2FieldNumber = 4;

			private double unknown2_;

			public const int Unknown3FieldNumber = 5;

			private int unknown3_;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static MessageParser<QRScanResult> Parser => _parser;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static MessageDescriptor Descriptor => QRScanRespMessage.Descriptor.NestedTypes[0];

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			MessageDescriptor IMessage.Descriptor => Descriptor;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public ByteString Result
			{
				get
				{
					return result_;
				}
				set
				{
					result_ = ProtoPreconditions.CheckNotNull<ByteString>(value, "value");
				}
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public int Unknow0
			{
				get
				{
					return unknow0_;
				}
				set
				{
					unknow0_ = value;
				}
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public double Unknown1
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
			public double Unknown2
			{
				get
				{
					return unknown2_;
				}
				set
				{
					unknown2_ = value;
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
			public QRScanResult()
			{
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public QRScanResult(QRScanResult other)
				: this()
			{
				result_ = other.result_;
				unknow0_ = other.unknow0_;
				unknown1_ = other.unknown1_;
				unknown2_ = other.unknown2_;
				unknown3_ = other.unknown3_;
				_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public QRScanResult Clone()
			{
				return new QRScanResult(this);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public override bool Equals(object other)
			{
				return Equals(other as QRScanResult);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public bool Equals(QRScanResult other)
			{
				if (other == null)
				{
					return false;
				}
				if (other == this)
				{
					return true;
				}
				if (Result != other.Result)
				{
					return false;
				}
				if (Unknow0 != other.Unknow0)
				{
					return false;
				}
				if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Unknown1, other.Unknown1))
				{
					return false;
				}
				if (!ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Unknown2, other.Unknown2))
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
				if (Result.Length != 0)
				{
					num ^= ((object)Result).GetHashCode();
				}
				if (Unknow0 != 0)
				{
					num ^= Unknow0.GetHashCode();
				}
				if (Unknown1 != 0.0)
				{
					num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Unknown1);
				}
				if (Unknown2 != 0.0)
				{
					num ^= ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Unknown2);
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
				if (Result.Length != 0)
				{
					output.WriteRawTag((byte)10);
					output.WriteBytes(Result);
				}
				if (Unknow0 != 0)
				{
					output.WriteRawTag((byte)16);
					output.WriteInt32(Unknow0);
				}
				if (Unknown1 != 0.0)
				{
					output.WriteRawTag((byte)25);
					output.WriteDouble(Unknown1);
				}
				if (Unknown2 != 0.0)
				{
					output.WriteRawTag((byte)33);
					output.WriteDouble(Unknown2);
				}
				if (Unknown3 != 0)
				{
					output.WriteRawTag((byte)40);
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
				if (Result.Length != 0)
				{
					num += 1 + CodedOutputStream.ComputeBytesSize(Result);
				}
				if (Unknow0 != 0)
				{
					num += 1 + CodedOutputStream.ComputeInt32Size(Unknow0);
				}
				if (Unknown1 != 0.0)
				{
					num += 9;
				}
				if (Unknown2 != 0.0)
				{
					num += 9;
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
			public void MergeFrom(QRScanResult other)
			{
				if (other != null)
				{
					if (other.Result.Length != 0)
					{
						Result = other.Result;
					}
					if (other.Unknow0 != 0)
					{
						Unknow0 = other.Unknow0;
					}
					if (other.Unknown1 != 0.0)
					{
						Unknown1 = other.Unknown1;
					}
					if (other.Unknown2 != 0.0)
					{
						Unknown2 = other.Unknown2;
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
						Result = input.ReadBytes();
						break;
					case 16u:
						Unknow0 = input.ReadInt32();
						break;
					case 25u:
						Unknown1 = input.ReadDouble();
						break;
					case 33u:
						Unknown2 = input.ReadDouble();
						break;
					case 40u:
						Unknown3 = input.ReadInt32();
						break;
					}
				}
			}
		}
	}

	private static readonly MessageParser<QRScanRespMessage> _parser = new MessageParser<QRScanRespMessage>((Func<QRScanRespMessage>)(() => new QRScanRespMessage()));

	private UnknownFieldSet _unknownFields;

	public const int QrResultFieldNumber = 1;

	private static readonly FieldCodec<Types.QRScanResult> _repeated_qrResult_codec = FieldCodec.ForMessage<Types.QRScanResult>(10u, Types.QRScanResult.Parser);

	private readonly RepeatedField<Types.QRScanResult> qrResult_ = new RepeatedField<Types.QRScanResult>();

	public const int Unknown0FieldNumber = 2;

	private int unknown0_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<QRScanRespMessage> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => UtilityProtobufReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<Types.QRScanResult> QrResult => qrResult_;

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
	public QRScanRespMessage()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRScanRespMessage(QRScanRespMessage other)
		: this()
	{
		qrResult_ = other.qrResult_.Clone();
		unknown0_ = other.unknown0_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public QRScanRespMessage Clone()
	{
		return new QRScanRespMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as QRScanRespMessage);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(QRScanRespMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!qrResult_.Equals(other.qrResult_))
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
		num ^= ((object)qrResult_).GetHashCode();
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
		qrResult_.WriteTo(ref output, _repeated_qrResult_codec);
		if (Unknown0 != 0)
		{
			output.WriteRawTag((byte)16);
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
		num += qrResult_.CalculateSize(_repeated_qrResult_codec);
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
	public void MergeFrom(QRScanRespMessage other)
	{
		if (other != null)
		{
			qrResult_.Add((IEnumerable<Types.QRScanResult>)other.qrResult_);
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
				qrResult_.AddEntriesFrom(ref input, _repeated_qrResult_codec);
				break;
			case 16u:
				Unknown0 = input.ReadInt32();
				break;
			}
		}
	}
}
