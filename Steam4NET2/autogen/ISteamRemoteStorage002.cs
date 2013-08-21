// This file is automatically generated.
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Steam4NET
{

	[StructLayout(LayoutKind.Sequential,Pack=4)]
	public class ISteamRemoteStorage002VTable
	{
		public IntPtr FileWrite0;
		public IntPtr GetFileSize1;
		public IntPtr FileRead2;
		public IntPtr FileExists3;
		public IntPtr GetFileCount4;
		public IntPtr GetFileNameAndSize5;
		public IntPtr GetQuota6;
		private IntPtr DTorISteamRemoteStorage0027;
	};
	
	[InteropHelp.InterfaceVersion("STEAMREMOTESTORAGE_INTERFACE_VERSION002")]
	public class ISteamRemoteStorage002 : InteropHelp.NativeWrapper<ISteamRemoteStorage002VTable>
	{
		[return: MarshalAs(UnmanagedType.I1)]
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate bool NativeFileWriteSBI( IntPtr thisptr, string pchFile, Byte[] pvData, Int32 cubData );
		public bool FileWrite( string pchFile, Byte[] pvData ) 
		{
			return this.GetFunction<NativeFileWriteSBI>( this.Functions.FileWrite0 )( this.ObjectAddress, pchFile, pvData, (Int32) pvData.Length ); 
		}
		
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate Int32 NativeGetFileSizeS( IntPtr thisptr, string pchFile );
		public Int32 GetFileSize( string pchFile ) 
		{
			return this.GetFunction<NativeGetFileSizeS>( this.Functions.GetFileSize1 )( this.ObjectAddress, pchFile ); 
		}
		
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate Int32 NativeFileReadSBI( IntPtr thisptr, string pchFile, Byte[] pvData, Int32 cubDataToRead );
		public Int32 FileRead( string pchFile, Byte[] pvData ) 
		{
			return this.GetFunction<NativeFileReadSBI>( this.Functions.FileRead2 )( this.ObjectAddress, pchFile, pvData, (Int32) pvData.Length ); 
		}
		
		[return: MarshalAs(UnmanagedType.I1)]
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate bool NativeFileExistsS( IntPtr thisptr, string pchFile );
		public bool FileExists( string pchFile ) 
		{
			return this.GetFunction<NativeFileExistsS>( this.Functions.FileExists3 )( this.ObjectAddress, pchFile ); 
		}
		
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate Int32 NativeGetFileCount( IntPtr thisptr );
		public Int32 GetFileCount(  ) 
		{
			return this.GetFunction<NativeGetFileCount>( this.Functions.GetFileCount4 )( this.ObjectAddress ); 
		}
		
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate string NativeGetFileNameAndSizeII( IntPtr thisptr, Int32 iFile, ref Int32 pnFileSizeInBytes );
		public string GetFileNameAndSize( Int32 iFile, ref Int32 pnFileSizeInBytes ) 
		{
			return InteropHelp.DecodeANSIReturn( this.GetFunction<NativeGetFileNameAndSizeII>( this.Functions.GetFileNameAndSize5 )( this.ObjectAddress, iFile, ref pnFileSizeInBytes ) ); 
		}
		
		[return: MarshalAs(UnmanagedType.I1)]
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)] private delegate bool NativeGetQuotaII( IntPtr thisptr, ref Int32 pnTotalBytes, ref Int32 puAvailableBytes );
		public bool GetQuota( ref Int32 pnTotalBytes, ref Int32 puAvailableBytes ) 
		{
			return this.GetFunction<NativeGetQuotaII>( this.Functions.GetQuota6 )( this.ObjectAddress, ref pnTotalBytes, ref puAvailableBytes ); 
		}
		
	};
}
