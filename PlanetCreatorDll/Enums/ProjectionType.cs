namespace PlanetGeneratorDll.Enums
{
    public enum ProjectionType
    {
        Mercator,               // Mercator projection 
        Peter,                  // Peters projection (area preserving cylindrical) 
        Square,                 // Square projection (equidistant latitudes) 
        Stereo,                 // Stereographic projection 
        Orthographic           // Orthographic projection MAIN
    }

    /*
     *     public enum ProjectionType
    {
        Mercator,               // Mercator projection 
        Peter,                  // Peters projection (area preserving cylindrical) 
        Square,                 // Square projection (equidistant latitudes) 
        Mollweide,              // Mollweide projection (area preserving) 
        Sinusoid,               // Sinusoid projection (area preserving) 
        Stereo,                 // Stereographic projection 
        Orthographic,           // Orthographic projection MAIN
        Gnomonic,               // Gnomonic projection 
        Icosahedral,            // Icosahedral projection 
        Azimuth,                // Area preserving azimuthal projection 
        Conical,                // Conical projection (conformal) 
        Heightfield,            // heightfield 
    }
     */
}