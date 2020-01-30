using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CXLaser
{
	public struct CALI
	{
		QX h;
		QX v;
	}
	struct QX
	{
		float p1, p2, p3, p4, p5, p6;
	}

	public static class UdpClass
	{
		/*
		 1    0 00001000 ?add@@YAHHH @Z
          2    1 00001630 ?del_socket@@YAXXZ
          3    2 00001110 ?get_cali_para@@YAHPAUCALI@@@Z
          4    3 00001500 ?rec_cam_line_data@@YAHPAM0PAUCALI@@@Z
          5    4 000013C0? rec_init@@YAHPAUCALI@@@Z
          6    5 00001650 ?set_laser_state@@YAH_N @Z
          7    6 00001830 ?set_weld_mode@@YAHE @Z
		  */
		//------------------------------------------------------------------------------------------------------
		//函数申明
		//数据接收初始化
		[DllImport("laser_tacker_dll.dll", EntryPoint = "?rec_init@@YAHPAUCALI@@@Z", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern int rec_init(ref CALI p);
		[DllImport("laser_tacker_dll.dll", EntryPoint = "?get_cali_para@@YAHPAUCALI@@@Z", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern int get_cali_para(CALI* p);
		[DllImport("laser_tacker_dll.dll", EntryPoint = "?del_socket@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
		public static extern void del_socket();
		//获取线激光轮廓数据
		//激光轮廓数据920个数据
		//x：激光线左右方向数据
		//z：激光线高低方向数据
		//p：激光线标定数据
		[DllImport("laser_tacker_dll.dll", EntryPoint = "?rec_cam_line_data@@YAHPAM0PAUCALI@@@Z", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern int rec_cam_line_data([MarshalAs(UnmanagedType.LPArray, SizeConst = 92)]float[] x, [MarshalAs(UnmanagedType.LPArray, SizeConst = 92)]float[] z, ref CALI p);
		//		public unsafe static extern int rec_cam_line_data(ref float x, ref float z, ref CALI p);
		[DllImport("laser_tacker_dll.dll", EntryPoint = "?set_laser_state@@YAH_N@Z", CallingConvention = CallingConvention.Cdecl)]
		public static extern int set_laser_state(bool laser_state);
		[DllImport("laser_tacker_dll.dll", EntryPoint = "?set_weld_mode@@YAHE@Z", CallingConvention = CallingConvention.Cdecl)]
		public static extern int set_weld_mode(string weld_mode);
		
	}
}
