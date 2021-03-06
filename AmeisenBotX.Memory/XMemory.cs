﻿using AmeisenBotX.Memory.Win32;
using Fasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static AmeisenBotX.Memory.Win32.Win32Imports;

namespace AmeisenBotX.Memory
{
    public unsafe class XMemory
    {
        private readonly object allocLock = new object();
        private ulong rpmCalls;
        private ulong wpmCalls;

        public XMemory()
        {
            MemoryAllocations = new Dictionary<IntPtr, uint>();
        }

        ~XMemory()
        {
            CloseHandle(MainThreadHandle);
            CloseHandle(ProcessHandle);

            List<IntPtr> memAllocs = MemoryAllocations.Keys.ToList();

            for (int i = 0; i < memAllocs.Count; ++i)
            {
                FreeMemory(memAllocs[i]);
            }
        }

        public int AllocationCount
            => MemoryAllocations.Count;

        public ManagedFasm Fasm { get; private set; }

        public IntPtr MainThreadHandle { get; private set; }

        public Dictionary<IntPtr, uint> MemoryAllocations { get; }

        public Process Process { get; private set; }

        public IntPtr ProcessHandle { get; private set; }

        public ulong RpmCallCount
        {
            get
            {
                unchecked
                {
                    ulong val = rpmCalls;
                    rpmCalls = 0;
                    return val;
                }
            }
        }

        public ulong WpmCallCount
        {
            get
            {
                unchecked
                {
                    ulong val = wpmCalls;
                    wpmCalls = 0;
                    return val;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BringWindowToFront(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoActivate;
            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetForegroundWindow()
        {
            return Win32Imports.GetForegroundWindow();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect GetWindowPosition(IntPtr windowHandle)
        {
            Rect rect = new Rect();
            GetWindowRect(windowHandle, ref rect);
            return rect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetForegroundWindow(IntPtr windowHandle)
        {
            Win32Imports.SetForegroundWindow(windowHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWindowPosition(IntPtr windowHandle, Rect rect, bool resizeWindow = true)
        {
            WindowFlags flags = WindowFlags.AsyncWindowPos | WindowFlags.NoZOrder | WindowFlags.NoActivate;
            if (!resizeWindow) { flags |= WindowFlags.NoSize; }

            if (rect.Left > 0 && rect.Right > 0 && rect.Top > 0 && rect.Bottom > 0)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, (int)flags);
            }
        }

        public static Process StartProcessNoActivate(string processCmd)
        {
            StartupInfo startupInfo = new StartupInfo
            {
                cb = Marshal.SizeOf<StartupInfo>(),
                dwFlags = STARTF_USESHOWWINDOW,
                wShowWindow = SW_SHOWMINNOACTIVE
            };

            if (CreateProcess(null, processCmd, IntPtr.Zero, IntPtr.Zero, true, 0x10, IntPtr.Zero, null, ref startupInfo, out ProcessInformation processInformation))
            {
                CloseHandle(processInformation.hProcess);
                CloseHandle(processInformation.hThread);

                return Process.GetProcessById(processInformation.dwProcessId);
            }
            else
            {
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllocateMemory(uint size, out IntPtr address)
        {
            lock (allocLock)
            {
                address = VirtualAllocEx(ProcessHandle, IntPtr.Zero, size, AllocationTypes.Commit, MemoryProtectionFlags.ExecuteReadWrite);
                if (address != IntPtr.Zero)
                {
                    MemoryAllocations.Add(address, size);
                    return true;
                }

                address = IntPtr.Zero;
                return false;
            }
        }

        public bool Attach(Process wowProcess)
        {
            Process = wowProcess;
            if (Process == null || Process.HasExited)
            {
                return false;
            }

            ProcessHandle = OpenProcess(ProcessAccessFlags.All, false, wowProcess.Id);
            if (ProcessHandle == IntPtr.Zero)
            {
                return false;
            }

            Fasm = new ManagedFasm(ProcessHandle);
            MemoryAllocations.Clear();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FreeMemory(IntPtr address)
        {
            lock (allocLock)
            {
                if (MemoryAllocations.ContainsKey(address)
                    && VirtualFreeEx(ProcessHandle, address, 0, AllocationTypes.Release))
                {
                    MemoryAllocations.Remove(address);
                    return true;
                }

                return false;
            }
        }

        public ProcessThread GetMainThread()
        {
            if (Process?.MainWindowHandle == null) { return null; }

            int id = GetWindowThreadProcessId(Process.MainWindowHandle, 0);

            for (int i = 0; i < Process.Threads.Count; ++i)
            {
                if (Process.Threads[i].Id == id)
                {
                    return Process.Threads[i];
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitmap GetScreenshot()
        {
            Rect rc = new Rect();
            GetWindowRect(Process.MainWindowHandle, ref rc);

            Bitmap bmp = new Bitmap(rc.Right - rc.Left, rc.Bottom - rc.Top, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rc.Left, rc.Top, 0, 0, new Size(rc.Right - rc.Left, rc.Bottom - rc.Top));
            }

            return bmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MemoryProtect(IntPtr address, uint size, MemoryProtectionFlags memoryProtection, out MemoryProtectionFlags oldMemoryProtection)
        {
            return VirtualProtectEx(ProcessHandle, address, size, memoryProtection, out oldMemoryProtection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PatchMemory<T>(IntPtr address, T data) where T : unmanaged
        {
            uint size = (uint)sizeof(T);

            if (MemoryProtect(address, size, MemoryProtectionFlags.ExecuteReadWrite, out MemoryProtectionFlags oldMemoryProtection))
            {
                Write(address, data);
                MemoryProtect(address, size, oldMemoryProtection, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read<T>(IntPtr address, T* value) where T : unmanaged
        {
            if (RpmGateWay(address, value, sizeof(T)))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read<T>(IntPtr address, out T value) where T : unmanaged
        {
            int size = sizeof(T);

            fixed (byte* pBuffer = stackalloc byte[size])
            {
                if (RpmGateWay(address, pBuffer, size))
                {
                    value = *(T*)pBuffer;
                    return true;
                }
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBytes(IntPtr address, int size, out byte[] bytes)
        {
            byte[] buffer = new byte[size];

            fixed (byte* pBuffer = buffer)
            {
                if (RpmGateWay(address, pBuffer, size))
                {
                    bytes = buffer;
                    return true;
                }
            }

            bytes = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadString(IntPtr address, Encoding encoding, out string value, int lenght = 128)
        {
            fixed (byte* pBuffer = stackalloc byte[lenght])
            {
                if (RpmGateWay(address, pBuffer, lenght))
                {
                    List<byte> strBuffer = new List<byte>(lenght);

                    for (int i = 0; i < lenght; ++i)
                    {
                        if (pBuffer[i] == 0) { break; }
                        strBuffer.Add(pBuffer[i]);
                    }

                    value = encoding.GetString(strBuffer.ToArray());
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadStruct<T>(IntPtr address, T* value) where T : unmanaged
        {
            if (RpmGateWay(address, value, sizeof(T)))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadStruct<T>(IntPtr address, out T value) where T : unmanaged
        {
            int size = sizeof(T);

            fixed (byte* pBuffer = stackalloc byte[size])
            {
                if (RpmGateWay(address, pBuffer, size))
                {
                    value = *(T*)pBuffer;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void ResizeParentWindow(int offsetX, int offsetY, int width, int height)
        {
            if (Process == null)
            {
                return;
            }

            SetWindowPos(Process.MainWindowHandle, IntPtr.Zero, offsetX, offsetY, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResumeMainThread()
        {
            if (MainThreadHandle != IntPtr.Zero || TryOpenMainThread(ThreadAccessFlags.SuspendResume))
            {
                NtResumeThread(MainThreadHandle, out _);
            }
        }

        public void SetupAutoPosition(IntPtr mainWindowHandle, int offsetX, int offsetY, int width, int height)
        {
            if (Process.MainWindowHandle != IntPtr.Zero && mainWindowHandle != IntPtr.Zero)
            {
                SetParent(Process.MainWindowHandle, mainWindowHandle);

                int style = GetWindowLong(Process.MainWindowHandle, GWL_STYLE) & ~(int)WindowStyles.WS_CAPTION & ~(int)WindowStyles.WS_THICKFRAME;
                SetWindowLong(Process.MainWindowHandle, GWL_STYLE, style);

                ResizeParentWindow(offsetX, offsetY, width, height);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendMainThread()
        {
            if (MainThreadHandle != IntPtr.Zero || TryOpenMainThread(ThreadAccessFlags.SuspendResume))
            {
                NtSuspendThread(MainThreadHandle, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write<T>(IntPtr address, T value) where T : unmanaged
        {
            return WpmGateWay(address, &value, sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
            {
                return WpmGateWay(address, pBytes, bytes.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZeroMemory(IntPtr address, int size)
        {
            return WriteBytes(address, new byte[size]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RpmGateWay(IntPtr baseAddress, void* buffer, int size)
        {
            ++rpmCalls;
            return !NtReadVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }

        private bool TryOpenMainThread(ThreadAccessFlags threadAccess)
        {
            try
            {
                ProcessThread processThread = GetMainThread();

                if (processThread != null)
                {
                    MainThreadHandle = OpenThread(threadAccess, false, (uint)processThread.Id);
                }
            }
            catch { }

            return MainThreadHandle != IntPtr.Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WpmGateWay(IntPtr baseAddress, void* buffer, int size)
        {
            ++wpmCalls;
            return !NtWriteVirtualMemory(ProcessHandle, baseAddress, buffer, size, out _);
        }
    }
}