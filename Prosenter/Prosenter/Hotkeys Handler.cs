
namespace Prosenter
{

    //Hotkeys DLL Import
    using System.Runtime.InteropServices;
    using System;
    using System.Windows.Interop;
    using System.Diagnostics;

    public partial class MainWindow
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID1 = 9000; //PGUP
        private const int HOTKEY_ID2 = 9001; //PGDN
        private const int HOTKEY_ID3 = 9002; //HOME
        private const int HOTKEY_ID4 = 9003; //END

        //Modifiers:
        private const uint MOD_CONTROL = 0x0002; // Ctrl key modifier

        //VKey bindings
        public static uint VK_UpArrow { get; set; } = 0x26;      // Up Arrow
        public static uint VK_DownArrow { get; set; } = 0x28;    // Down Arrow
        public static uint VK_ENTER { get; set; } = 0x0D;        // Enter
        public static uint VK_BACKSPACE { get; set; } = 0x08;    // Backspace

        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHookUP);
            _source.AddHook(HwndHookDOWN);
            _source.AddHook(HwndHookEND);
            _source.AddHook(HwndHookHOME);

            RegisterHotKey(_windowHandle, HOTKEY_ID1, MOD_CONTROL, VK_UpArrow); //PAGEUP
            RegisterHotKey(_windowHandle, HOTKEY_ID2, MOD_CONTROL, VK_DownArrow); //PAGEDOWN
            RegisterHotKey(_windowHandle, HOTKEY_ID3, MOD_CONTROL, VK_ENTER); //HOME
            RegisterHotKey(_windowHandle, HOTKEY_ID4, MOD_CONTROL, VK_BACKSPACE); //END
            Debug.WriteLine("Hotkeys registered succesfully");

        }


        //PAGEUP Handler
        private IntPtr HwndHookUP(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID1)
            {
                int vkey = (int)lParam >> 16 & 0xFFFF;
                if (vkey == VK_UpArrow)
                {
                    Debug.WriteLine("PageUp was pressed");
                    Debug.WriteLine(PresentationItems == null ? "PresentationItems is empty" : "PresentationItems is not empty");
                    cycleUp();
                }

                handled = true;
            }

            Debug.WriteLine("Hotkey Pressed");
            return IntPtr.Zero;
        }

        //PAGEDOWN Handler
        private IntPtr HwndHookDOWN(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID2:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_DownArrow)
                            {
                                Debug.WriteLine("PageDown was pressed");
                                cycleDown();
                            }
                            handled = true;
                            break;

                    }
                    break;
            }
            return IntPtr.Zero;
        }

        //HOME Handler
        private IntPtr HwndHookHOME(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID3:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_ENTER)
                            {
                                Debug.WriteLine("PAUSE/Start was pressed");
                                if (status == "Stopped")
                                {
                                    StoppedToStaging();
                                    break;
                                }
                                if (status == "Staging")
                                {
                                    StagingToStarted();
                                    break;
                                }
                                if (status == "Started")
                                {
                                    StartedToPaused();
                                    break;
                                }
                                if (status == "Paused")
                                {
                                    PausedToStarted();
                                    break;
                                }

                            }
                            handled = true;
                            break;

                    }
                    break;
            }
            return IntPtr.Zero;
        }

        //END Handler
        private IntPtr HwndHookEND(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID4:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_BACKSPACE)
                            {
                                Debug.WriteLine("END was pressed");
                                if (status == "Staging")
                                {
                                    StagingToStopped();
                                    break;
                                }
                                if (status == "Started")
                                {
                                    StartedToStopped();
                                    break;

                                }
                                if (status == "Paused")
                                {
                                    PausedToStopped();
                                    break;
                                }
                            }
                            handled = true;
                            break;

                    }
                    break;
            }
            return IntPtr.Zero;
        }


        //unregisters the hotkeys on close
        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHookUP);
            _source.RemoveHook(HwndHookDOWN);
            _source.RemoveHook(HwndHookHOME);
            _source.RemoveHook(HwndHookEND);
            UnregisterHotKey(_windowHandle, HOTKEY_ID1);
            UnregisterHotKey(_windowHandle, HOTKEY_ID2);
            UnregisterHotKey(_windowHandle, HOTKEY_ID3);
            UnregisterHotKey(_windowHandle, HOTKEY_ID4);
            base.OnClosed(e);
        }
        public void assignHotkeys()
        {
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHookUP);
            _source.AddHook(HwndHookDOWN);
            _source.AddHook(HwndHookEND);
            _source.AddHook(HwndHookHOME);

            RegisterHotKey(_windowHandle, HOTKEY_ID1, MOD_CONTROL, VK_UpArrow); //PAGEUP
            RegisterHotKey(_windowHandle, HOTKEY_ID2, MOD_CONTROL, VK_DownArrow); //PAGEDOWN
            RegisterHotKey(_windowHandle, HOTKEY_ID3, MOD_CONTROL, VK_ENTER); //HOME
            RegisterHotKey(_windowHandle, HOTKEY_ID4, MOD_CONTROL, VK_BACKSPACE); //END}
        }

    }
}

