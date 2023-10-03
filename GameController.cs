using SplashKitSDK;

public class GameController
{
    private Window _GameWindow;
    private Bitmap _BackgroundBitmap;
    private Bitmap _GroundBitmap;
    private float _groundX = 0; // Set initial ground position and scrolling speed
    private float _groundSpeed = 2;
    public bool Quit { get; private set; } // property to tell the program to stop running the game

    private Player _Player;

    // Create a list to store game objects
    private List<GameObject> _GameObjects = new List<GameObject>();
    private List<GameObject> _GameObjectsToRemove = new List<GameObject>();

    // The gap that exists between pipe pairs for players to fly through
    private int _GAP;

    // Create a random number generator
    private Random _rnd = new Random();

    // Player score variable
    private double _playerScore = 0;
    private double _highScore = 0;

    // Boolean to start the game session in a paused state
    private bool _isPaused;
    private bool _isGameOver;
    private bool _isGameOverBitmapDrawn;
    private Bitmap _GameOverBitmap;

    // Constructor to create the game controller
    // We will perform all initializations here
    public GameController()
    {
        _BackgroundBitmap = new Bitmap("Background", "images/Background.png");
        _GroundBitmap = new Bitmap("Ground", "Ground.png");
        _GameOverBitmap = new Bitmap("GameOver", "GameOver.png");
        _GameWindow = new Window("Climby Bird", _BackgroundBitmap.Width, _BackgroundBitmap.Height);
        _Player = new Player(_GameWindow);
        _GAP = _Player.Height * 3; // Wide enough to fit thrice the height of the player
        Quit = false;
        _isPaused = true;
        _isGameOver = false;
        _isGameOverBitmapDrawn = false;
    }

    public void HandleInput()
    {
        // Quit the game if the escape key is pressed or the window is closed
        if (SplashKit.KeyDown(KeyCode.EscapeKey) || _GameWindow.CloseRequested)
        {
            Quit = true;
        }
        // Start the game if it is paused, or reset if the game is over
        if (SplashKit.KeyDown(KeyCode.SpaceKey))
        {
            _isPaused = false;
            if (_isGameOver)
            {
                ResetGame();
            }
        }
    }

    // Draws all active game objects on screen
    public void Draw()
    {
        // When the game is over, we draw the GameOver bitmap and prevent any GameObject interaction
        if (_isGameOver)
        {
            if (!_isGameOverBitmapDrawn)
            {
                _GameOverBitmap.Draw((_GameWindow.Width - _GameOverBitmap.Width) / 2, (_GameWindow.Height - _GameOverBitmap.Height) / 2 - 40);
                _GameWindow.Refresh(60);
            }
            return;
        }

        _GameWindow.Clear(Color.White);
        _BackgroundBitmap.Draw(0, 0);

        // When the game is paused, we know the game session just began. We will only draw the background, the floor, and the player and
        // wait for the player to start the game
        if (_isPaused)
        {
            _Player.Draw();
            _Player.MaintainGround(_GameWindow.Height - _GroundBitmap.Height);

            // Draw the ground multiple times to create the illusion of infinite scrolling
            for (float x = _groundX; x < _GameWindow.Width; x += _GroundBitmap.Width)
            {
                _GameWindow.DrawBitmap(_GroundBitmap, x, _GameWindow.Height - _GroundBitmap.Height);
            }

            SplashKit.DrawText("" + _playerScore, Color.White, "Score", 32, _GameWindow.Width / 2, 20);

            _GameWindow.Refresh(60);
            return;
        }

        // Spawn pipes
        // Here we're checking if the space between the last added pipe and the end of the screen is above a certain amount (half the game window height)
        // If it is, then we add a new pair of pipes.
        if (_GameWindow.Width - (_GameObjects.Count > 0 ? (_GameObjects[_GameObjects.Count - 1] is Coin ? _GameObjects[_GameObjects.Count - 2].X : _GameObjects[_GameObjects.Count - 1].X) : 0) > _GameWindow.Height / 2)
        {
            // We've already set the _GAP (between pipe pairs) to be thrice the height of the player. Here we are trying to calculate the point
            // on the y-axis where we will begin to draw this gap. We have bounds from the top of the window, to the ground level.
            // We use the Random() object to generate a random value from this bound, with a padding of 100 on either side so the gap isn't
            // overly close to the edges to give our players a fair fight.
            int gapY = _rnd.Next(100, _GameWindow.Height - _GAP - _GroundBitmap.Height - 100); // Generate a random gapY

            // Once we get this gap origin, we place the bottom of the topPipe (read that again...bottom of the topPipe) at this origin point,
            // and the top of the bottomPipe (need I say it again?), the _GAP point away from this origin point.
            GameObject topPipe = new TopPipe(_GameWindow.Width, gapY);
            GameObject bottomPipe = new BottomPipe(_GameWindow.Width, gapY + _GAP);

            _GameObjects.Add(topPipe);
            _GameObjects.Add(bottomPipe);
        }
        else
        {
            // Spawn a coin
            // To spawn a coin, we just check if there is no coin on scene, and that enough space (1/4 of the game window height)
            // exists between our current position and the last added pipe pair
            if (!_GameObjects.Any(gameObject => gameObject is Coin) && _GameWindow.Width - (_GameObjects.Count > 0 ? _GameObjects[_GameObjects.Count - 1].X : 0) > _GameWindow.Height / 4)
            {
                // The padding here is reduced to make it tempting but not too easy for players to get the coins
                int coinY = _rnd.Next(80, _GameWindow.Height - _GroundBitmap.Height - 80); // Generate a random coinY
                GameObject coin = new Coin(_GameWindow.Width, coinY);
                _GameObjects.Add(coin);
            }
        }

        _GameObjects.ForEach(gameObject =>
        {
            gameObject.Draw();

            // Thanks to Polymorphism, we can distinguish between GameObjects in our list
            if (gameObject is Coin)
            {
                // We want to only check for collisions with coins, however that happens
                // When there is a collision, we want to remove the coin from the scene, and reward the player
                if (new Collision(_Player.CollidedWith)(gameObject))
                {
                    _GameObjectsToRemove.Add(gameObject);
                    _playerScore += 3;
                }
            }
            else
            {
                // Here, we check for collisions with pipes; the only other type of GameObject we have
                // First, we want to check if the player is close to a pair of pipes
                // If the player is within range of a pair of pipes, one of two things will happen; either they collide with the pipe,
                // or they pass through the gap successfully.
                if (new Collision(_Player.IsWithinGameObjectBounds)(gameObject))
                {
                    // We check if the player passes through successfully
                    if (new Collision(_Player.PassesGap)(gameObject))
                    {
                        // Since this will count for the pair of pipes, we can increment score by 0.5
                        _playerScore += 0.5;
                    }
                    else if (new Collision(_Player.CollidedWith)(gameObject))
                    {
                        // Game over
                        _isGameOver = true;
                        return;
                    }
                }
            }

            // Remove game objects that go off-screen
            if (gameObject.IsOffScreen())
            {
                _GameObjectsToRemove.Add(gameObject);
            }
        });

        _Player.Draw();
        _Player.HandleInput();
        _Player.MaintainGround(_GameWindow.Height - _GroundBitmap.Height);

        _GameObjectsToRemove.ForEach(gameObject =>
        {
            _GameObjects.Remove(gameObject);
        });

        // Update ground position
        _groundX -= _groundSpeed;

        // Reset ground position if it goes off-screen
        if (_groundX <= -_GroundBitmap.Width)
        {
            _groundX = 0;
        }

        // Draw the ground multiple times to create the illusion of infinite scrolling
        for (float x = _groundX; x < _GameWindow.Width; x += _GroundBitmap.Width)
        {
            _GameWindow.DrawBitmap(_GroundBitmap, x, _GameWindow.Height - _GroundBitmap.Height);
        }

        // Draw the player's score on the screen with the Score font
        SplashKit.DrawText("" + _playerScore, Color.White, "Score", 32, _GameWindow.Width / 2, 20);
        if (_highScore > 0)
        {
            SplashKit.DrawText("High Score: " + _highScore, Color.White, "Score", 12, 10, 20);
        }
        _GameWindow.Refresh(60);
    }

    // Resets the game so the player can start again
    private void ResetGame()
    {
        _Player = new Player(_GameWindow);
        // Keep a record of the highest score for the game session
        if (_playerScore > _highScore)
        {
            _highScore = _playerScore;
        }
        _playerScore = 0;
        _GameObjects = new List<GameObject>();
        _GameObjectsToRemove = new List<GameObject>();
        _isGameOver = false;
    }
}