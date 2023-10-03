using SplashKitSDK;

public abstract class GameObject
{
    public abstract Bitmap _ObjectBitmap { get; }
    public double X { get; private set; }
    public virtual double Y { get; protected set; }

    public const int SPEED = 2;

    public GameObject(double x)
    {
        X = x;
    }

    public virtual void Draw()
    {
        X -= SPEED; // Constantly reduce the x-axis value so the object translates from right to left
    }

    public bool IsOffScreen()
    {
        return X + _ObjectBitmap.Width < 0; // Return if the object is out of view
    }
}

public class TopPipe : GameObject
{
    private Bitmap _pipeBitmap;
    public override Bitmap _ObjectBitmap { get { return _pipeBitmap; } }

    public TopPipe(double x, double y) : base(x)
    {
        _pipeBitmap = new Bitmap("TopPipe", "TopPipe.png");
        Y = 0 - (_pipeBitmap.Height - y);
    }

    override public void Draw()
    {
        base.Draw();
        _pipeBitmap.Draw(X, Y);
    }
}

public class BottomPipe : GameObject
{
    private Bitmap _pipeBitmap;
    public override Bitmap _ObjectBitmap { get { return _pipeBitmap; } }

    public BottomPipe(double x, double y) : base(x)
    {
        _pipeBitmap = new Bitmap("BottomPipe", "BottomPipe.png");
        Y = y;
    }

    override public void Draw()
    {
        base.Draw();
        _pipeBitmap.Draw(X, Y);
    }
}

public class Coin : GameObject
{
    private Bitmap _coinBitmap;
    public override Bitmap _ObjectBitmap { get { return _coinBitmap; } }
    private float _rotationAngle = 0.0f;

    public Coin(double x, double y) : base(x)
    {
        _coinBitmap = new Bitmap("Coin", "Coin.png");
        Y = y;
    }

    override public void Draw()
    {
        base.Draw();
        // Update the rotation angle
        _rotationAngle += 2.0f;

        // Limit the rotation angle to 360 degrees to prevent overflow
        if (_rotationAngle >= 360.0f)
        {
            _rotationAngle = 0.0f;
        }
        _coinBitmap.Draw(X, Y, SplashKit.OptionRotateBmp(_rotationAngle));
    }
}