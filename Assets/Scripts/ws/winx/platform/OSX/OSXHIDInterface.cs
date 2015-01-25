//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.IO;


#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using System.Collections.Generic;
using ws.winx.devices;
using System.Runtime.InteropServices;
using System.Linq;


namespace ws.winx.platform.osx
{

   

    using UnityEngine; 
	using CFAllocatorRef=System.IntPtr;
	using CFDictionaryRef=System.IntPtr;
	using CFStringRef=System.IntPtr;
	using CFNumberRef=System.IntPtr;
	using CFArrayCallBacks=System.IntPtr;
	using CFArrayRef = System.IntPtr;
	using CFTypeRef = System.IntPtr;
	using IOHIDDeviceRef = System.IntPtr;
	using IOHIDElementRef = System.IntPtr;
	using IOHIDManagerRef = System.IntPtr;
	using IOHIDValueRef = System.IntPtr;
	using IOOptionBits = System.IntPtr;
	using IOReturn = Native.IOReturn;//System.IntPtr;
	using IOHIDElementCookie = System.UInt32;
	using CFTypeID=System.UInt64;
	using CFIndex =System.Int64;



    sealed class OSXHIDInterface : IHIDInterface,IDisposable
    {

#region Fields

        readonly IOHIDManagerRef hidmanager;


        readonly IntPtr RunLoop = Native.CFRunLoopGetMain();
		readonly IntPtr InputLoopMode = Native.RunLoopModeDefault;
        readonly Native.CFArray DeviceTypes;

        Native.IOHIDDeviceCallback HandleHIDDeviceAdded;
        Native.IOHIDDeviceCallback HandleHIDDeviceRemoved;
		Native.IOHIDCallback HandleDeviceRemoved;
     

        bool disposed;
		bool hidCallbacksRegistered;

		private static readonly object syncRoot = new object();
		private List<IDriver> __drivers;

		private DeviceProfiles __profiles;


        private IDriver __defaultJoystickDriver;

		private string[] __ports;
	

      
		private Dictionary<string, HIDDevice> __Generics;
		
		
		public event EventHandler<DeviceEventArgs<string>> DeviceDisconnectEvent;
		
		public event EventHandler<DeviceEventArgs<IDevice>> DeviceConnectEvent;



#endregion


		
		
#region Contsructor
		
		public OSXHIDInterface()
		{
			__drivers = new List<IDriver>();

			__ports = new string[20];


			
			HandleHIDDeviceAdded = HidDeviceAdded;
			HandleHIDDeviceRemoved = HidDeviceRemoved;
			HandleDeviceRemoved = DeviceRemoved;
			
			CFDictionaryRef[] dictionaries;
			
			
			
			
			
			
			dictionaries = new CFDictionaryRef[3];
			
			//create 3 search patterns by Joystick,GamePad and MulitAxisController
			
			// base.typeRef = CFLibrary.CFDictionaryCreate(IntPtr.Zero,keyz,values,keys.Length,ref kcall,ref vcall); 
			
			
			
			
			dictionaries[0] = CreateDeviceMatchingDictionary((uint)Native.HIDPage.GenericDesktop,(uint)Native.HIDUsageGD.Joystick);			
			
			dictionaries[1] = CreateDeviceMatchingDictionary((uint)Native.HIDPage.GenericDesktop,(uint)Native.HIDUsageGD.GamePad);
			
			dictionaries[2] = CreateDeviceMatchingDictionary((uint)Native.HIDPage.GenericDesktop,(uint)Native.HIDUsageGD.MultiAxisController);
			
			
			
			
			
			
			DeviceTypes= new Native.CFArray (dictionaries);
			
			
			//create Hid manager	
			hidmanager = Native.IOHIDManagerCreate(IntPtr.Zero,(int)Native.IOHIDOptionsType.kIOHIDOptionsTypeNone);
			
			

			
			
			
			
		}
        #endregion

#region IHIDInterface implementation


		public void SetProfiles(DeviceProfiles profiles){

			__profiles = profiles;
		}

		public void LoadProfiles(string fileName){

			__profiles=Resources.Load<DeviceProfiles> ("DeviceProfiles");

		}
	

		public DeviceProfile LoadProfile(string key){

			DeviceProfile profile=null;

			if (__profiles.vidpidProfileNameDict.ContainsKey (key)) {

				string profileName=__profiles.vidpidProfileNameDict[key];



				if(__profiles.runtimePlatformDeviceProfileDict[profileName].ContainsKey(RuntimePlatform.OSXPlayer)){

					profile=__profiles.runtimePlatformDeviceProfileDict[profileName][RuntimePlatform.OSXPlayer];
				}

			}
			

			return profile;
		}


		public void AddDriver (IDriver driver)
		{
			__drivers.Add (driver);
		}





		public bool Contains (string id)
		{
			return __Generics != null && __Generics.ContainsKey (id);
		}



		public void Enumerate(){


			if (hidmanager != IntPtr.Zero) {	
				
				if (__Generics != null) {

					foreach (KeyValuePair<string, HIDDevice> entry in __Generics) {
						entry.Value.Dispose ();
					}
					
					
					__Generics.Clear ();

				}else
				__Generics = new Dictionary<string, HIDDevice>();


				if(!hidCallbacksRegistered){
					//Register add/remove device handlers
					RegisterHIDCallbacks(hidmanager);


				}
				
				
			}else
				UnityEngine.Debug.LogError("Creating of OSX HIDManager failed");         

				}

		public HIDReport ReadDefault(string id){
            return Generics[id].ReadDefault(); 
		}

        public HIDReport ReadBuffered(string id){
			return Generics [id].ReadBuffered ();
		}

		public void Read (string id, HIDDevice.ReadCallback callback)
		{
			throw new NotImplementedException ();
		}

		public void Read (string id, HIDDevice.ReadCallback callback, int timeout)
		{
			throw new NotImplementedException ();
		}

		public void Write (object data, string id, HIDDevice.WriteCallback callback, int timeout)
		{
			throw new NotImplementedException ();
		}

		public void Write (object data, string id, HIDDevice.WriteCallback callback)
		{
			__Generics [id].Write (data, callback);
		}

		public void Write (object data, string id)
		{
				__Generics [id].Write (data);
		}



		public Dictionary<string, HIDDevice> Generics {
			get {
				return __Generics;
			}
		}


        public IDriver defaultDriver
        {
            get { if (__defaultJoystickDriver == null) { __defaultJoystickDriver = new OSXDriver(); } return __defaultJoystickDriver; }
            set { __defaultJoystickDriver = value; 
				if(value is ws.winx.drivers.UnityDriver){

					Debug.LogWarning("UnityDriver set as default driver.\n Warring:Unity doesn't make distinction between triggers/axis/pow, samekind controllers can't be distinct as they would have same na in GetJoystickList and some controlers would have different joystick index then on in list. Also current profiles aren't done for this driver");
					//LoadProfiles("profiles_uni.txt");
				}
			}

        }




      





        public void Update()
        {
        }

#endregion







#region Private Members

		static CFDictionaryRef CreateDeviceMatchingDictionary(uint inUsagePage, uint inUsage)
		{
				IntPtr pageCFNumberRef = (new Native.CFNumber ((int)inUsagePage)).typeRef;
				IntPtr usageCFNumberRef = (new Native.CFNumber ((int)inUsage)).typeRef;
				CFStringRef[] keys;
				keys = new IntPtr[2];
				keys [0] = Native.CFSTR (Native.kIOHIDDeviceUsagePageKey);//new Native.CFString(Native.IOHIDDeviceUsagePageKey);
				keys [1] = Native.CFSTR (Native.kIOHIDDeviceUsageKey);//new Native.CFString(Native.IOHIDDeviceUsageKey);

				Native.CFDictionary dict = new Native.CFDictionary (keys, new IntPtr[] { 
				pageCFNumberRef,usageCFNumberRef});
			
				return dict.typeRef;
		}
			

		// Registers callbacks for device addition and removal. These callbacks
		// are called when we run the loop in CheckDevicesMode
		void RegisterHIDCallbacks(IOHIDManagerRef hidmanager)
		{
			try{
				UnityEngine.Debug.Log("OSXHIDInterface> RegisterHIDCallbacks");

				Native.IOHIDManagerRegisterDeviceMatchingCallback(
                hidmanager, HandleHIDDeviceAdded, IntPtr.Zero);
            Native.IOHIDManagerRegisterDeviceRemovalCallback(
                hidmanager, HandleHIDDeviceRemoved, IntPtr.Zero);
            Native.IOHIDManagerScheduleWithRunLoop(hidmanager,RunLoop, InputLoopMode);

            //Native.IOHIDManagerSetDeviceMatching(hidmanager, DeviceTypes.Ref);
            Native.IOHIDManagerSetDeviceMatchingMultiple(hidmanager, DeviceTypes.typeRef);

				Native.CFRelease(DeviceTypes.typeRef);


            IOReturn result=Native.IOHIDManagerOpen(hidmanager, (int)Native.IOHIDOptionsType.kIOHIDOptionsTypeNone);

				if(result==IOReturn.kIOReturnSuccess){
					Native.CFRunLoopRunInMode(InputLoopMode, 0.0, true);

					hidCallbacksRegistered=true;
				}else{
					UnityEngine.Debug.LogError("OSXHIDInterface can't open hidmanager! Error:"+result);
				}



            

			}catch(Exception ex){
				UnityEngine.Debug.LogException(ex);
						}
           
        }


        /// <summary>
        /// Devices the added.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="res">Res.</param>
        /// <param name="sender">Sender.</param>
        /// <param name="device">Device.</param>
        void HidDeviceAdded(IntPtr context, IOReturn res, IntPtr sender, IOHIDDeviceRef deviceRef)
        {
			//IOReturn success = Native.IOHIDDeviceOpen (device, (int)Native.IOHIDOptionsType.kIOHIDOptionsTypeNone);

			if (deviceRef == IntPtr.Zero) {
				Debug.LogWarning("IOHIDeviceRef of Added Device equal to IntPtr.Zero");
				return;
			}




				int product_id = (int)(new Native.CFNumber(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDProductIDKey)))).ToInteger();

				

				int vendor_id =(int)(new Native.CFNumber(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDVendorIDKey)))).ToInteger();

				string manufacturer=(new Native.CFString(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDManufacturerKey)))).ToString();
				string description =manufacturer+" "+(new Native.CFString(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDProductKey)))).ToString();

				int location=(int)(new Native.CFNumber(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDLocationIDKey)))).ToInteger();
				string transport=(new Native.CFString(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDTransportKey)))).ToString();

				string path=String.Format("{0:s}_{1,4:X}_{2,4:X}_{3:X}",
				                          transport, vendor_id, product_id, location);//"%s_%04hx_%04hx_%x"
					
			//string serial=(new Native.CFString(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDSerialNumberKey)))).ToString();

			if(Generics.ContainsKey(path)) return;

				GenericHIDDevice hidDevice;
				IDevice joyDevice = null;
               // IDevice<IAxisDetails, IButtonDetails, IDeviceExtension> joyDevice = null;


				///loop thru specific drivers and attach the driver to device if compatible
				if (__drivers != null)
					foreach (var driver in __drivers)
                {

					

				hidDevice=new GenericHIDDevice(GetIndexForDeviceWithID(path),vendor_id, product_id, path,deviceRef, this,path,description);

                    if ((joyDevice = driver.ResolveDevice(hidDevice)) != null)
					{

						lock(syncRoot){
							__Generics[path] = hidDevice;
						}

					//if (context != IntPtr.Zero) {
					Native.IOHIDDeviceRegisterRemovalCallback(deviceRef,HandleDeviceRemoved,context);
					//}else{
					//	Debug.LogWarning("IOHIDDeviceRegisterRemovalCallback not registerd cos of Context IntPtr.Zero");
					//}

					Debug.Log("Device PID:" + joyDevice.PID + " VID:" + joyDevice.VID +  "["+joyDevice.Name+"] attached to " + driver.GetType().ToString());

                        break;
                    }
                }

                if (joyDevice == null)
                {//set default driver as resolver if no custom driver match device

					

					hidDevice=new GenericHIDDevice(GetIndexForDeviceWithID(path),vendor_id, product_id,path, deviceRef, this,path,description);


					if ((joyDevice = defaultDriver.ResolveDevice(hidDevice)) != null)
                    {
                       
						lock(syncRoot){
						__Generics[path] = hidDevice;
						}

						//if (context != IntPtr.Zero) {
							Native.IOHIDDeviceRegisterRemovalCallback(deviceRef,HandleDeviceRemoved,context);
						//}else{
						//	Debug.LogWarning("IOHIDDeviceRegisterRemovalCallback not registerd cos of Context IntPtr.Zero");
						//}
						
						Debug.Log("Device PID:" + joyDevice.PID + " VID:" + joyDevice.VID + "["+joyDevice.Name+"] attached to " + defaultDriver.GetType().ToString());

                       
                    }
                    else
				    {
                        Debug.LogWarning("Device PID:" + product_id.ToString() + " VID:" + vendor_id.ToString() + " not found compatible driver on the system.Removed!");
                    
                    }


			



                }

			if(joyDevice!=null)
			this.DeviceConnectEvent(this,new DeviceEventArgs<IDevice>(joyDevice));



            
        }


		/// <summary>
		/// Gets the index for device with ID.
		/// </summary>
		/// <returns>Old index for device after reconnection or new if first connection.</returns>
		/// <param name="ID">ID(probably devicePath)</param>
		int GetIndexForDeviceWithID(string ID){
			int inx;
			//find if this device was using same port before (during same app runitime)
			inx=Array.IndexOf(__ports,ID);
			
			if(inx<0)//if not found => use next available(20 ports in total) position
				inx=Array.IndexOf(__ports,null);

			__ports [inx] = ID;

			return inx;

		}

		void DeviceRemoved(IntPtr inContext, IOReturn result, IntPtr sender)
		{
						RemoveDevice (sender);
		}


        /// <summary>
        /// Devices the removed.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="res">Res.</param>
        /// <param name="sender">Sender.</param>
        /// <param name="device">Device.</param>
        void HidDeviceRemoved(IntPtr context, IOReturn res, IntPtr sender, IOHIDDeviceRef deviceRef)
        {
           
			            
						RemoveDevice (deviceRef);
		    
          
        }


		void RemoveDevice(IOHIDDeviceRef deviceRef){

						if (deviceRef == IntPtr.Zero) {
							Debug.LogWarning("IOHIDeviceRef equal to IntPtr.Zero");
							return;
						}
						
			
			int product_id = (int)(new Native.CFNumber(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDProductIDKey)))).ToInteger();
			
			
			
			int vendor_id =(int)(new Native.CFNumber(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDVendorIDKey)))).ToInteger();
			

			int location=(int)(new Native.CFNumber(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDLocationIDKey)))).ToInteger();
			string transport=(new Native.CFString(Native.IOHIDDeviceGetProperty(deviceRef, Native.CFSTR(Native.kIOHIDTransportKey)))).ToString();
			
			string path=String.Format("{0:s}_{1,4:X}_{2,4:X}_{3:X}",
			                          transport, vendor_id, product_id, location);//"%s_%04hx_%04hx_%x"


						lock (syncRoot) {
						if (!__Generics.ContainsKey (path))
								return;
						
						HIDDevice hidDevice = __Generics [path];
		
						
						Generics.Remove (path);

						Debug.Log ("Device "+hidDevice.index+" PID:" + hidDevice.PID + " VID:" + hidDevice.VID + " Disconnected");

						}


						Debug.Log ("Try to unshedule,unregister and close device");
						Native.IOHIDDeviceUnscheduleFromRunLoop(deviceRef, RunLoop, InputLoopMode);
						Native.IOHIDDeviceRegisterInputValueCallback(deviceRef,IntPtr.Zero,IntPtr.Zero);
						Native.IOHIDDeviceRegisterRemovalCallback (deviceRef, null, IntPtr.Zero);
						
						Native.IOHIDDeviceClose (deviceRef,(int)Native.IOHIDOptionsType.kIOHIDOptionsTypeNone);
						
						

						this.DeviceDisconnectEvent(this,new DeviceEventArgs<string>(path));

		}


#endregion










       




        void IDisposable.Dispose()
        {
           if (hidmanager != IntPtr.Zero) {

				//throw exception don't know why
//				if(RunLoop!=IntPtr.Zero && InputLoopMode!=IntPtr.Zero)
//				Native.IOHIDDeviceUnscheduleWithRunLoop(hidmanager,
//				                                        RunLoop, InputLoopMode);


				Debug.Log ("Try to remove OSXHIDInterface registers");

				Native.IOHIDManagerRegisterDeviceMatchingCallback(
					hidmanager, IntPtr.Zero, IntPtr.Zero);
				Native.IOHIDManagerRegisterDeviceRemovalCallback(
					hidmanager, IntPtr.Zero, IntPtr.Zero);

				Debug.Log ("Try to release HIDManager");
				Native.CFRelease(hidmanager);
			}

            lock(syncRoot){

						if (Generics != null) {
								foreach (KeyValuePair<string, HIDDevice> entry in Generics) {
										entry.Value.Dispose ();
								}
			

								Generics.Clear ();
						}


			Debug.Log ("Try to remove Drivers");
			if(__drivers!=null) __drivers.Clear();
        }


				      
        }
    }
}

#endif