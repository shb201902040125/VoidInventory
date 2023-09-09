using SDL2;

namespace VoidInventory
{
    public static class VIMessageBox
    {
        public enum VIMessageBoxButtons
        {
                        OK,
                        OKCancel,
                        YesNoCancel,
                        YesNo,
                        RetryCancel
        }
        public enum VIDialogResult
        {
                        None,
                        OK,
                        Cancel,
                        Abort,
                        Retry,
                        Ignore,
                        Yes,
                        No
        }
        public enum VIMessageBoxIcon : uint
        {
                        None,
                        Error = 16U,
                        Hand = 16U,
                        Stop = 16U,
                        Exclamation = 32U,
                        Warning = 32U,
                        Asterisk = 64U,
                        Information = 64U
        }
                public static VIDialogResult Show(string msg, string title, VIMessageBoxButtons buttons = VIMessageBoxButtons.OK, VIMessageBoxIcon icon = VIMessageBoxIcon.None)
        {
            SDL.SDL_MessageBoxData sdl_MessageBoxData = default(SDL.SDL_MessageBoxData);
            sdl_MessageBoxData.flags = (SDL.SDL_MessageBoxFlags)icon;
            sdl_MessageBoxData.message = msg;
            sdl_MessageBoxData.title = title;
            SDL.SDL_MessageBoxButtonData[] buttons2 = buttons switch
            {
                VIMessageBoxButtons.OK => new SDL.SDL_MessageBoxButtonData[]
                                    {
                    OKButton
                                    },
                VIMessageBoxButtons.OKCancel => new SDL.SDL_MessageBoxButtonData[]
                    {
                    CancelButton,
                    OKButton
                    },
                VIMessageBoxButtons.YesNoCancel => new SDL.SDL_MessageBoxButtonData[]
                    {
                    CancelButton,
                    NoButton,
                    YesButton
                    },
                VIMessageBoxButtons.YesNo => new SDL.SDL_MessageBoxButtonData[]
                    {
                    NoButton,
                    YesButton
                    },
                VIMessageBoxButtons.RetryCancel => new SDL.SDL_MessageBoxButtonData[]
                    {
                    CancelButton,
                    RetryButton
                    },
                _ => throw new NotImplementedException(),
            };
            sdl_MessageBoxData.buttons = buttons2;
            SDL.SDL_MessageBoxData msgBox = sdl_MessageBoxData;
            msgBox.numbuttons = msgBox.buttons.Length;
            SDL.SDL_ShowMessageBox(ref msgBox, out int buttonid);
            return (VIDialogResult)buttonid;
        }

                private static SDL.SDL_MessageBoxButtonData OKButton = new SDL.SDL_MessageBoxButtonData
        {
            flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
            buttonid = 1,
            text = "OK"
        };

                private static SDL.SDL_MessageBoxButtonData CancelButton = new SDL.SDL_MessageBoxButtonData
        {
            flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT,
            buttonid = 2,
            text = "Cancel"
        };

                private static SDL.SDL_MessageBoxButtonData YesButton = new SDL.SDL_MessageBoxButtonData
        {
            flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
            buttonid = 6,
            text = "Yes"
        };

                private static SDL.SDL_MessageBoxButtonData NoButton = new SDL.SDL_MessageBoxButtonData
        {
            buttonid = 7,
            text = "No"
        };

                private static SDL.SDL_MessageBoxButtonData RetryButton = new SDL.SDL_MessageBoxButtonData
        {
            flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
            buttonid = 4,
            text = "Retry"
        };
    }
}
