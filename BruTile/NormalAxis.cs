﻿#region License

// Copyright 2008 - Paul den Dulk (Geodan)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

#endregion

using System;

namespace BruTile
{
    internal class NormalAxis : IAxis
    {
        public TileRange WorldToTile(Extent extent, int level, ITileSchema schema)
        {
            var resolution = schema.Resolutions[level];
            var tileWorldUnits = resolution.UnitsPerPixel * schema.Width;
            var firstCol = (int)Math.Floor((extent.MinX - schema.OriginX) / tileWorldUnits);
            var firstRow = (int)Math.Floor((extent.MinY - schema.OriginY) / tileWorldUnits);
            var lastCol = (int)Math.Ceiling((extent.MaxX - schema.OriginX) / tileWorldUnits);
            var lastRow = (int)Math.Ceiling((extent.MaxY - schema.OriginY) / tileWorldUnits);
            return new TileRange(firstCol, firstRow, lastCol, lastRow);
        }

        public Extent TileToWorld(TileRange range, int level, ITileSchema schema)
        {
            var resolution = schema.Resolutions[level];
            var tileWorldUnits = resolution.UnitsPerPixel * schema.Width;
            var minX = range.FirstCol * tileWorldUnits + schema.OriginX;
            var minY = range.FirstRow * tileWorldUnits + schema.OriginY;
            var maxX = (range.LastCol + 1) * tileWorldUnits + schema.OriginX;
            var maxY = (range.LastRow + 1) * tileWorldUnits + schema.OriginY;
            return new Extent(minX, minY, maxX, maxY);
        }
    }
}
