using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace XAMLSnake
{
    /// <summary>
    /// Logique d'interaction pour XAMLSnakeWindow.xaml
    /// </summary>
    public partial class XAMLSnakeWindow : Window
    {
        // This list describes the Bonus Red pieces of Food on the Canvas
        private List<Point> bonusPoints = new List<Point>();
        // This list describes the body of the snake on the Canvas
        private List<Point> snakePoints = new List<Point>();
        private enum MOVINGDIRECTION
        {
            UPWARDS = 8,
            DOWNWARDS = 2,
            TOLEFT = 4,
            TORIGHT = 6
        };
        // Change au resize 
        private int XSIZE;
        private int YSIZE;
        //CONFIG 
        private int headSize = 64;
        private int NbBonus = 15;
        //Only one will stay
        private TimeSpan FAST = new TimeSpan(1000);
        private TimeSpan MODERATE = new TimeSpan(10000);
        private TimeSpan SLOW = new TimeSpan(100000);
        private TimeSpan DAMNSLOW = new TimeSpan(1000000);
        private Point startingPoint = new Point();
        private Point currentPosition = new Point();
        //BG must always be at pos 0
        //bonus at pos 1-nbBonus
        //Head at pos NbBonus+1
        //Body follow
        // Movement direction initialisation
        private int direction = 0;
        /* Placeholder for the next movement direction
         * used to move Case by case
         * The snake needs this to avoid its own body.  */
        private int nextDirection = 0;
        /* Placeholder for the previous movement direction
         * The snake needs this to avoid its own body.  */
        private int previousDirection = 0;
        private int length;
        private int score = 0;
        private Random rnd = new Random();
        DispatcherTimer _timer;
        private Stopwatch _stopwatch;
        private ImageBrush _headBrush;
        private ImageBrush _bodyBrush;
        private ImageBrush _bonusBrush;
        private ImageBrush _bgBrush;

        public XAMLSnakeWindow()
        {
            InitializeComponent();
            Loaded += LoadGame;
            KeyDown += new KeyEventHandler(OnButtonKeyDown);
        }
        private void LoadGame(object sender, RoutedEventArgs e)
        {
            YSIZE = (int)paintCanvas.ActualHeight;
            XSIZE = (int)paintCanvas.ActualWidth;
            paintBackground();
            //On load un brush
            BitmapImage headImg = new BitmapImage(new Uri("Images/SnakeHEad.png", UriKind.Relative));
            _headBrush = new ImageBrush(headImg);
            BitmapImage bodyImg = new BitmapImage(new Uri("Images/SnakeBody.png", UriKind.Relative));
            _bodyBrush = new ImageBrush(bodyImg);
            BitmapImage bonusImg = new BitmapImage(new Uri("Images/SnakeBonus.png", UriKind.Relative));
            _bonusBrush = new ImageBrush(bonusImg);
            BitmapImage bgImg = new BitmapImage(new Uri("Images/SnakeBg.png", UriKind.Relative));
            _bgBrush = new ImageBrush(bgImg);
            length = headSize * 3;
            _stopwatch = Stopwatch.StartNew();
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(timer_Tick);
            /* Here user can change the speed of the snake. 
             * Possible speeds are FAST, MODERATE, SLOW and DAMNSLOW */
            _timer.Interval = FAST;
            _timer.Start();
            paintBackground();
            startingPoint.X = (int)(paintCanvas.ActualWidth / 2);
            startingPoint.Y = (int)(paintCanvas.ActualHeight / 2);
            // Instantiate Food Objects
            for (int n = 0; n < NbBonus; n++)
            {
                paintBonus(n);
            }
            paintSnake(startingPoint);
            currentPosition = startingPoint;
        }
        private void paintBackground()
        {
            Rectangle newRectangle = new Rectangle();
            newRectangle.Fill = _bgBrush;
            newRectangle.Width = XSIZE;
            newRectangle.Height = YSIZE;
            Canvas.SetTop(newRectangle, 0);
            Canvas.SetLeft(newRectangle, 0);
            if (paintCanvas.Children.Count > 0)
            {
                paintCanvas.Children.RemoveAt(0);
            }
            paintCanvas.Children.Insert(0, newRectangle);
        }
        private void paintSnake(Point currentposition)
        {
            cleanSnake();
            paintSnakeHead(currentposition);
            paintSnakeBody(currentposition);
        }
        private void cleanSnake()
        {
            var count = paintCanvas.Children.Count;
            var toDel = count - (NbBonus + 1);
            paintCanvas.Children.RemoveRange(NbBonus + 1, toDel);
        }
        private void paintSnakeHead(Point currentposition)
        {
            var rot = GetRotation();
            RotateTransform aRotateTransform = new RotateTransform();
            aRotateTransform.CenterX = 0.5;
            aRotateTransform.CenterY = 0.5;
            aRotateTransform.Angle = rot;
            _headBrush.RelativeTransform = aRotateTransform;
            Rectangle headRect = new Rectangle();
            headRect.Fill = _headBrush;
            headRect.Height = headSize;
            headRect.Width = headSize;
            Canvas.SetTop(headRect, currentposition.Y);
            Canvas.SetLeft(headRect, currentposition.X);
            //On decale tout les point du snake
            snakePoints.Insert(0, currentposition);
            paintCanvas.Children.Add(headRect);
            int count = snakePoints.Count;
            // Restrict the tail of the snake
            if (count > length)
            {
                snakePoints.RemoveAt(length);
            }
        }
        private int GetRotation()
        {
            switch (direction)
            {
                case (int)MOVINGDIRECTION.DOWNWARDS:
                    return 180;
                case (int)MOVINGDIRECTION.UPWARDS:
                    return 0;
                case (int)MOVINGDIRECTION.TOLEFT:
                    return 270;
                case (int)MOVINGDIRECTION.TORIGHT:
                    return 90;
                default:
                    return 0;
            }
        }
        private void paintSnakeBody(Point currentposition)
        {
            for (int i = 1; i < snakePoints.Count; i++)
            {
                //On ne dessinne un cercle que tout les headsize position
                if (i % headSize == 0)
                {
                    Rectangle bodyRect = new Rectangle();
                    bodyRect.Fill = _bodyBrush;
                    bodyRect.Height = headSize;
                    bodyRect.Width = headSize;
                    Canvas.SetTop(bodyRect, snakePoints[i].Y);
                    Canvas.SetLeft(bodyRect, snakePoints[i].X);
                    paintCanvas.Children.Add(bodyRect);
                }
            }
        }
        private void paintBonus(int index)
        {
            Point bonusPoint = new Point(rnd.Next(headSize, XSIZE), rnd.Next(headSize, YSIZE));
            bonusPoint.X = bonusPoint.X - bonusPoint.X % headSize;
            bonusPoint.Y = bonusPoint.Y - bonusPoint.Y % headSize;
            Rectangle bodyRect = new Rectangle();
            bodyRect.Fill = _bonusBrush;
            bodyRect.Height = headSize;
            bodyRect.Width = headSize;
            Canvas.SetTop(bodyRect, bonusPoint.Y);
            Canvas.SetLeft(bodyRect, bonusPoint.X);
            paintCanvas.Children.Insert(index + 1, bodyRect);
            bonusPoints.Insert(index, bonusPoint);
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            var previousPosition = currentPosition;
            // Expand the body of the snake to the direction of movement
            switch (direction)
            {
                case (int)MOVINGDIRECTION.DOWNWARDS:
                    currentPosition.Y += 1;
                    if (currentPosition.Y % headSize == 0)
                    {
                        previousDirection = direction;
                        direction = nextDirection;
                    }
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.UPWARDS:
                    currentPosition.Y -= 1;
                    if (currentPosition.Y % headSize == 0)
                    {
                        previousDirection = direction;
                        direction = nextDirection;
                    }
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TOLEFT:
                    currentPosition.X -= 1;
                    if (currentPosition.X % headSize == 0)
                    {
                        previousDirection = direction;
                        direction = nextDirection;
                    }
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TORIGHT:
                    currentPosition.X += 1;
                    if (currentPosition.X % headSize == 0)
                    {
                        previousDirection = direction;
                        direction = nextDirection;
                    }
                    paintSnake(currentPosition);
                    break;
            }
            // Restrict to boundaries of the Canvas
            if ((currentPosition.X < 0) || (currentPosition.X > XSIZE) ||
                (currentPosition.Y < 0) || (currentPosition.Y > YSIZE))
                GameOver();
            // Hitting a bonus Point causes the lengthen-Snake Effect
            int n = 0;
            foreach (Point point in bonusPoints)
            {
                if ((Math.Abs(point.X - currentPosition.X) < headSize) &&
                    (Math.Abs(point.Y - currentPosition.Y) < headSize))
                {
                    length += headSize;
                    score += 10;
                    // In the case of food consumption, erase the food object
                    // from the list of bonuses as well as from the canvas
                    bonusPoints.RemoveAt(n);
                    paintCanvas.Children.RemoveAt(n + 1);
                    paintBonus(n);
                    break;
                }
                n++;
            }
            // Restrict hits to body of Snake
            for (int q = headSize * 2; q < snakePoints.Count; q++)
            {
                Point point = new Point(snakePoints[q].X, snakePoints[q].Y);
                if ((Math.Abs(point.X - currentPosition.X) < (headSize)) &&
                     (Math.Abs(point.Y - currentPosition.Y) < (headSize)))
                {
                    GameOver();
                    break;
                }
            }
            var time = _stopwatch.Elapsed;
            var titleString = "Time " + new DateTime(time.Ticks).ToString("HH:mm:ss") + "       Score " + score;
            this.Title = titleString;
        }
        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (previousDirection != (int)MOVINGDIRECTION.UPWARDS)
                        nextDirection = (int)MOVINGDIRECTION.DOWNWARDS;
                    break;
                case Key.Up:
                    if (previousDirection != (int)MOVINGDIRECTION.DOWNWARDS)
                        nextDirection = (int)MOVINGDIRECTION.UPWARDS;
                    break;
                case Key.Left:
                    if (previousDirection != (int)MOVINGDIRECTION.TORIGHT)
                        nextDirection = (int)MOVINGDIRECTION.TOLEFT;
                    break;
                case Key.Right:
                    if (previousDirection != (int)MOVINGDIRECTION.TOLEFT)
                        nextDirection = (int)MOVINGDIRECTION.TORIGHT;
                    break;
                case Key.Escape:
                    _timer.Stop();
                    this.Close();
                    break;
            }
            if (direction == 0)
            {
                direction = nextDirection;
            }
        }
        private void GameOver()
        {
            _timer.Stop();
            var mb = MessageBox.Show("You Lose! Your score is " + score.ToString(), "Game Over", MessageBoxButton.OK, MessageBoxImage.Hand);
            if (mb == MessageBoxResult.OK)
            {
                this.Close();
            }
        }
    }
}
