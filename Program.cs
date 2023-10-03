using System;
using SplashKitSDK;

namespace ClimbyBird
{
    public class Program
    {
        public static void Main()
        {
            SplashKit.LoadFont("Score", "PressStart2P-Regular.ttf");
            GameController game = new GameController();
            do
            {
                SplashKit.ProcessEvents();
                game.HandleInput();
                game.Draw();
            } while (game.Quit == false);
        }
    }
}
