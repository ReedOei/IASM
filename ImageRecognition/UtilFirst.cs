delegate int Order(int Direction, int x, int y);

public Point GetColor(int X, int Y, int Direction, Color Find)
{
	return GetLine(X, Y, 1, Direction, Find);
}

public Point GetLine(int X, int Y, int Length, int Direction, Color Find)
{
	//0 is top down. This means we run through the x's at each height.
	Order GetCoordinateX = (D, PX, PY) => (D == 0) ? PY : PX;
	Order GetCoordinateY = (D, PX, PY) => (D == 0) ? PX : PY;
	
	for (int x = X; x < GetCoordinateX(Direction, Source.Width, Source.Height); x++)
	{
		for (int y = Y; y < GetCoordinateY(Direction, Source.Width, Source.Height); y++)
		{
			Match = true;
		
			for (int i = y; i < Length; i++)
			{
				if (!ImageUtility.ColorMatch(GetPixel(GetCoordinateX(Direction, x, i), GetCoordinateY(Direction, x, i)), Find))
					Match = false;
			}
		
			if (Match)
				return new Point(x, y);
		}
	}
	
	return null;
}