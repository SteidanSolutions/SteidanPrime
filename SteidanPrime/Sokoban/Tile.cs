using System;
using System.Collections.Generic;
using System.Text;

namespace SteidanPrime.Sokoban
{
    class Tile
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

        public void SetStatus(int Color, StatusType Status)
        {
            this.Color = Color;
            this.Status = Status;
        }

        public override string ToString()
        {
            if (Status == StatusType.GROUND)
                return ":black_large_square:";

            if (Status == StatusType.WALL)
                switch (Color)
                {
                    case 0:
                        return ":red_square:";
                    case 1:
                        return ":orange_square:";
                    case 2:
                        return ":yellow_square:";
                    case 3:
                        return ":green_square:";
                    case 4:
                        return ":blue_square:";
                    default:
                        return ":purple_square:";
                }

            if (Status == StatusType.BOX)
                return ":brown_square:";

            if (Status == StatusType.DESTINATION)
                return ":negative_squared_cross_mark:";

            return ":flushed:";
        }
    }
}
