﻿// Copyright 2008 - Paul den Dulk (Geodan)
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

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Globalization;

[assembly: CLSCompliant(true)]

namespace BruTile
{
    public enum AxisDirection
    {
        //Direction is relative to the coordinate system in which the map is presented.
        Normal,
        InvertedY
        //InvertedX and InvertedXY do not exist yet, and may never.
    }

    public class TileSchema : ITileSchema
    {
        #region Fields

        private readonly List<double> _resolutions = new List<double>();
        private AxisDirection _axisDirection = AxisDirection.Normal;
        IAxis _axis = new NormalAxis();

        public TileSchema()
        {
            OriginY = Double.NaN;
            OriginX = Double.NaN;
        }

        #endregion

        #region Properties

        //Todo: see if we can replace all setters with constructor arguments. Do this after automatic parser is implemented

        public string Name { get; set; }
        public string Srs { get; set; }
        public Extent Extent { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }

        public IList<double> Resolutions
        {
            get { return _resolutions; }
        }

        public AxisDirection Axis
        {
            get { return _axisDirection; }
            set 
            { 
                _axisDirection = value;
                _axis = CreateAxis(value);
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the TileSchema members are properly initialized and throws an exception if not.
        /// </summary>
        public virtual void Validate()
        {
            if (String.IsNullOrEmpty(Srs))
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "The SRS was not set for TileSchema '{0}'", Name));
            }
            if (Extent == new Extent())
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "The BoundingBox was not set for TileSchema '{0}'", Name));
            }
            if (Double.IsNaN(OriginX))
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "TileSchema {0} OriginX was 'not a number', perhaps it was not initialized.", Name));
            }
            if (Double.IsNaN(OriginY))
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "TileSchema {0} OriginY was 'not a number', perhaps it was not initialized.", Name));
            }
            if (_resolutions.Count == 0)
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "No Resolutions were added for TileSchema '{0}'", Name));
            }
            if (Width == 0)
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "The Width was not set for TileSchema '{0}'", Name));
            }
            if (Height == 0)
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "The Height was not set for TileSchema '{0}'", Name));
            }
            if (String.IsNullOrEmpty(Format))
            {
                throw new ValidationException(String.Format(CultureInfo.InvariantCulture,
                  "The Format was not set for TileSchema '{0}'", Name));
            }

            //TODO: BoundingBox should contain a SRS, and we should check if BoundingBox.Srs is the same
            //as TileSchema Srs because we do not project one to the other. 
        }

        /// <summary>
        /// Returns a List of TileInfos that cover the provided extent. 
        /// </summary>
        public IList<TileInfo> GetTilesInView(Extent extent, double resolution)
        {
            int level = Utilities.GetNearestLevel(Resolutions, resolution);
            return GetTilesInView(extent, level);
        }

        public IList<TileInfo> GetTilesInView(Extent extent, int level)
        {
            IList<TileInfo> infos = new List<TileInfo>();
            TileRange range = _axis.WorldToTile(extent, level, this);
            infos.Clear();

            for (int x = range.FirstCol; x < range.LastCol; x++)
            {
                for (int y = range.FirstRow; y < range.LastRow; y++)
                {
                    var info = new TileInfo();
                    info.Extent = _axis.TileToWorld(new TileRange(x, y), level, this);
                    info.Index = new TileIndex(x, y, level);

                    if (WithinSchemaExtent(Extent, info.Extent))
                    {
                        infos.Add(info);
                    }
                }
            }
            return infos;
        }

        public Extent GetExtentOfTilesInView(Extent extent, int level)
        {
            TileRange range = _axis.WorldToTile(extent, level, this);
            return _axis.TileToWorld(range, level, this);
        }

        #endregion

        #region Private Methods

        private static bool WithinSchemaExtent(Extent schemaExtent, Extent tileExtent)
        {
            //Always return false when tile is outsize of schema
            if (!tileExtent.Intersects(schemaExtent)) return false;

            //Do not always accept when the tile is partially inside the schema. 
            //Reject tiles that have less than 0.1% percent overlap.
            //In practice they turn out to be mostly false positives due to rounding errors.
            //They are not present on the server and the failed requests make slow the application down.
            return ((tileExtent.Intersect(schemaExtent).Area / tileExtent.Area) > 0.001);
        }

        private static IAxis CreateAxis(AxisDirection axis)
        {
            switch (axis)
            {
                case AxisDirection.Normal:
                    return new NormalAxis();
                case AxisDirection.InvertedY:
                    return new InvertedYAxis();
                default:
                    throw new ArgumentException("could not find axis transformer");
            }
        }

        #endregion
    }

}
