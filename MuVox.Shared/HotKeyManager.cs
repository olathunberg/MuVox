﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TTech.MuVox.Shared
{
    public static class HotKeyManager
    {
        public static event EventHandler<HotKeyEventArgs>? HotKeyPressed;

        public static int RegisterHotKey(Keys key, KeyModifiers modifiers)
        {
            int id = System.Threading.Interlocked.Increment(ref _id);
            NativeMethods.RegisterHotKey(_wnd.Handle, id, (uint)modifiers, (uint)key);
            return id;
        }

        public static bool UnregisterHotKey(int id)
        {
            return NativeMethods.UnregisterHotKey(_wnd.Handle, id);
        }

        public static void OnHotKeyPressed(HotKeyEventArgs e)
        {
            HotKeyPressed?.Invoke(null, e);
        }

        private static readonly MessageWindow _wnd = new MessageWindow();

        private class MessageWindow : Form
        {
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    var e = new HotKeyEventArgs(m.LParam);
                    OnHotKeyPressed(e);
                }

                base.WndProc(ref m);
            }

            private const int WM_HOTKEY = 0x312;
        }

        private static int _id = 0;

        private static class NativeMethods
        {
            [DllImport("user32")]
            internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32")]
            internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }
    }

    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            var param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }
}
