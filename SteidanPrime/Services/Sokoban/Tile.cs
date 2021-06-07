namespace SteidanPrime.Services.Sokoban
{
    public class Tile
    {
        public int Color { get; set; }
        public StatusType Status { get; set; }

        public Tile(int Color, StatusType Status)
        {
            this.Color = Color;
            this.Status = Status;
        }

        public Tile(StatusType Status)
        {
            this.Color = 0;
            this.Status = Status;
        }

        public override string ToString()
        {
            return Status switch
            {
                StatusType.GROUND => ":black_large_square:",
                StatusType.WALL => Color switch
                {
                    0 => ":red_square:",
                    1 => ":orange_square:",
                    2 => ":yellow_square:",
                    3 => ":green_square:",
                    4 => ":blue_square:",
                    _ => ":purple_square:"
                },
                StatusType.BOX => ":brown_square:",
                StatusType.DESTINATION => ":negative_squared_cross_mark:",
                _ => ":flushed:"
            };
        }
    }
}
