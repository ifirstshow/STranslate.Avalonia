using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace UtilityProtobuf;

public static class UtilityProtobufReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static UtilityProtobufReflection()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected Obj, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected Obj, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected Obj, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected Obj, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected Obj, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Expected Obj, but got Unknown
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Expected Obj, but got Unknown
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Expected Obj, but got Unknown
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Expected Obj, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chxwcm90by91dGlsaXR5X3Byb3RvYnVmLnByb3RvEhB1dGlsaXR5X3Byb3Rv" + "YnVmIjgKDkluaXRSZXFNZXNzYWdlEhgKEGV4cGlyZV90aW1lc3RhbXAYASAB" + "KAUSDAoEdHlwZRgCIAEoBSIhCg9Jbml0UmVzcE1lc3NhZ2USDgoGc3RhdHVz" + "GAEgASgFIngKF1Jlc2FtcGxlSW1hZ2VSZXFNZXNzYWdlEhIKCmRlY29kZV9w" + "aWMYASABKAkSEgoKZW5jb2RlX3BpYxgCIAEoCRIQCghyZV9waWNfeBgDIAEo" + "BRIQCghyZV9waWNfeRgEIAEoBRIRCgl1bmtub3duXzAYBSABKAUiLAoYUmVz" + "YW1wbGVJbWFnZVJlc3BNZXNzYWdlEhAKCGVycl9jb2RlGAEgASgFIm0KD1Rl" + "eHRTY2FuTWVzc2FnZRIUCgx0ZXh0X3NjYW5faWQYASABKAUSEAoIcGljX3Bh" + "dGgYAiABKAkSEQoJaGF2ZV90ZXh0GAMgASgFEhEKCXVua25vd25fMBgEIAEo" + "BRIMCgRyYXRlGAUgASgCIs4BChBRUlNjYW5SZXFNZXNzYWdlEhIKCm9yaWdp" + "bl9waWMYASABKAkSEgoKZGVjb2RlX3BpYxgCIAEoCRIRCgl1bmtub3duXzAY" + "AyABKAUSFwoPZW5jb2RlX3BpY19kYXRhGAQgASgMEhQKDGVuY29kZV9waWNf" + "eBgFIAEoBRIUCgxlbmNvZGVfcGljX3kYBiABKAUSEQoJdW5rbm93bl8xGAcg" + "ASgFEhQKDHRleHRfc2Nhbl9pZBgIIAEoBRIRCgl1bmtub3duXzMYCSABKAUi" + "1gEKEVFSU2NhblJlc3BNZXNzYWdlEkMKCXFyX3Jlc3VsdBgBIAMoCzIwLnV0" + "aWxpdHlfcHJvdG9idWYuUVJTY2FuUmVzcE1lc3NhZ2UuUVJTY2FuUmVzdWx0" + "EhEKCXVua25vd25fMBgCIAEoBRppCgxRUlNjYW5SZXN1bHQSDgoGcmVzdWx0" + "GAEgASgMEhAKCHVua25vd18wGAIgASgFEhEKCXVua25vd25fMRgDIAEoARIR" + "Cgl1bmtub3duXzIYBCABKAESEQoJdW5rbm93bl8zGAUgASgFYgZwcm90bzM="), new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, new GeneratedClrTypeInfo[7]
		{
			new GeneratedClrTypeInfo(typeof(InitReqMessage), (MessageParser)(object)InitReqMessage.Parser, new string[2] { "ExpireTimestamp", "Type" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null),
			new GeneratedClrTypeInfo(typeof(InitRespMessage), (MessageParser)(object)InitRespMessage.Parser, new string[1] { "Status" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null),
			new GeneratedClrTypeInfo(typeof(ResampleImageReqMessage), (MessageParser)(object)ResampleImageReqMessage.Parser, new string[5] { "DecodePic", "EncodePic", "RePicX", "RePicY", "Unknown0" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null),
			new GeneratedClrTypeInfo(typeof(ResampleImageRespMessage), (MessageParser)(object)ResampleImageRespMessage.Parser, new string[1] { "ErrCode" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null),
			new GeneratedClrTypeInfo(typeof(TextScanMessage), (MessageParser)(object)TextScanMessage.Parser, new string[5] { "TextScanId", "PicPath", "HaveText", "Unknown0", "Rate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null),
			new GeneratedClrTypeInfo(typeof(QRScanReqMessage), (MessageParser)(object)QRScanReqMessage.Parser, new string[9] { "OriginPic", "DecodePic", "Unknown0", "EncodePicData", "EncodePicX", "EncodePicY", "Unknown1", "TextScanId", "Unknown3" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null),
			new GeneratedClrTypeInfo(typeof(QRScanRespMessage), (MessageParser)(object)QRScanRespMessage.Parser, new string[2] { "QrResult", "Unknown0" }, (string[])null, (Type[])null, (Extension[])null, new GeneratedClrTypeInfo[1]
			{
				new GeneratedClrTypeInfo(typeof(QRScanRespMessage.Types.QRScanResult), (MessageParser)(object)QRScanRespMessage.Types.QRScanResult.Parser, new string[5] { "Result", "Unknow0", "Unknown1", "Unknown2", "Unknown3" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
			})
		}));
	}
}
