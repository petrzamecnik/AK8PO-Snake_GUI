using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class main : Node2D
{
    // props
    private int _score = 0;
    private const int CellSize = 32;
    private const double MoveInterval = 0.2;
    
    private Vector2 _velocity = new Vector2(32, 0);
    private double _timeElapsed = 0.0;
    private string _lastDirection = "ui_right";

    private int[] _widthConstraints = [32, 736];
    private int[] _heightConstraints = [64, 768];

    private RandomNumberGenerator _random = new RandomNumberGenerator();

    private CharacterBody2D _snakeHead;
    private List<Sprite2D> _snakeBody = new List<Sprite2D>();
    private Vector2 _lastSnakeHeadPosition;
    private Sprite2D _food;
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _generateFood();
        _snakeHead = GetNode<CharacterBody2D>("SnakeHead");
        GrowSnakeBody();
        GrowSnakeBody();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _timeElapsed += delta;
        HandleMovement();

        if (_timeElapsed >= MoveInterval)
        {
            _lastSnakeHeadPosition = _snakeHead.Position;
            _snakeHead.Position += _velocity;
            _timeElapsed = 0.0;

            // Check if the snake head collides with the food
            if (IsColliding(_snakeHead.Position, _food.Position))
            {
                EatFood();
            }



            // Move the snake body
            MoveSnakeBody();
        }
        
        if (IsCollidingWithBody() || IsInsideBackground(_snakeHead.Position))
        {
            // TODO: end game
            GetTree().Quit();

                
        }
    }


    private bool IsCollidingWithBody()
    {
        return _snakeBody.Any(bodyPart => IsColliding(_snakeHead.Position, bodyPart.Position));
    }
    
    private bool IsCollidingWithWalls()
    {
        var wallLeft = GetNode<Sprite2D>("WallLeft");
        var wallRight = GetNode<Sprite2D>("WallRight");
        var wallTop = GetNode<Sprite2D>("WallTop");
        var wallBottom = GetNode<Sprite2D>("WallBottom");
        
        var snakeHeadRect = new Rect2(_snakeHead.Position, new Vector2(0, 0));
        
        return snakeHeadRect.Intersects(wallLeft.GetRect())
               || snakeHeadRect.Intersects(wallRight.GetRect())
               || snakeHeadRect.Intersects(wallTop.GetRect())
               || snakeHeadRect.Intersects(wallBottom.GetRect());
    }

    private bool IsInsideBackground(Vector2 snakeHeadPosition)
    {
        if ((snakeHeadPosition.X < _widthConstraints[0] || snakeHeadPosition.X > _widthConstraints[1]) || (snakeHeadPosition.Y < _heightConstraints[0] || snakeHeadPosition.Y > _heightConstraints[1]))
        {
            return true;
        }
        
        return false;
    }

    private bool IsColliding(Vector2 position1, Vector2 position2)
    {
        return position1 == position2;
    }

    private void MoveSnakeBody()
    {
        // Move each body part to the position of the previous body part
        for (var i = _snakeBody.Count - 1; i > 0; i--)
        {
            _snakeBody[i].Position = _snakeBody[i - 1].Position;
        }

        // Move the first body part to the previous position of the snake head
        if (_snakeBody.Count > 0)
        {
            _snakeBody[0].Position = _lastSnakeHeadPosition;
        }
    }

    private void EatFood()
    {
        RemoveChild(_food);
        _food.QueueFree();
        _generateFood();
        GrowSnakeBody();
        _score++;
    }

    private void GrowSnakeBody()
    {
        var bodyTexture = GD.Load<Texture2D>("res://textures/snakeBody.png");
        var newBodyPart = new Sprite2D();
        newBodyPart.Texture = bodyTexture;
        newBodyPart.Position = _lastSnakeHeadPosition;
        _snakeBody.Add(newBodyPart);
        AddChild(newBodyPart);
    }

	private void HandleMovement()
	{
		
		if (Input.IsActionPressed("ui_right") && _lastDirection != "ui_left")
		{
			_velocity = new Vector2(32, 0); // Move right
			_lastDirection = "ui_right";
		}
		else if (Input.IsActionPressed("ui_left") && _lastDirection != "ui_right")
		{
			_velocity = new Vector2(-32, 0); // Move left
			_lastDirection = "ui_left";
		}
		else if (Input.IsActionPressed("ui_down") && _lastDirection != "ui_up")
		{
			_velocity = new Vector2(0, 32); // Move down
			_lastDirection = "ui_down";
		}
		else if (Input.IsActionPressed("ui_up") && _lastDirection != "ui_down")
		{
			_velocity = new Vector2(0, -32); // Move up
			_lastDirection = "ui_up";
		}
	}
    private string GetOppositeDirection(string direction)
    {
        switch (direction)
        {
            case "ui_right":
                return "ui_left";
            case "ui_left":
                return "ui_right";
            case "ui_down":
                return "ui_up";
            case "ui_up":
                return "ui_down";
            default:
                return "";
        }
    }
    private void _generateFood()
    {
        var foodTexture = GD.Load<Texture2D>("res://textures/food.png");
        _food = new Sprite2D();
        _food.Texture = foodTexture;
        _food.Position = _generateFoodPosition();
        AddChild(_food);
    }

    private Vector2 _generateFoodPosition()
    {
        // TODO: refactor this abomination

        // Calculate valid cell ranges
        var cellsWide = (_widthConstraints[1] - _widthConstraints[0]) / CellSize;
        var cellsHigh = (_heightConstraints[1] - _heightConstraints[0]) / CellSize;

        // Generate random cell position within range
        _random.Randomize(); // Initialize the random generator if needed
        var randomCellX = _random.RandiRange(_widthConstraints[0] / CellSize,
            cellsWide + _widthConstraints[0] / CellSize - 1);
        var randomCellY = _random.RandiRange(_heightConstraints[0] / CellSize,
            cellsHigh + _heightConstraints[0] / CellSize - 1);

        // Convert cell position to screen coordinates (align to cell center)
        return new Vector2(randomCellX * CellSize + CellSize / 2,
            randomCellY * CellSize + CellSize / 2);
    }
}