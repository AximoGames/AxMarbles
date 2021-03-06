
#ifdef USE_VERTEX_UV
matDiffuse = texture(material.DiffuseMap, TexCoords) * material.DiffuseColor;
#else
matDiffuse = material.DiffuseColor;
#endif

vec3 dir = NormalTransposed;
if(joker == 0.0)
{
    if(
        (dir.y>0 && dir.x>0 && dir.z>0)
        || (dir.y<0 && dir.x<0 && dir.z>0)
        ||  (dir.y>0 && dir.x<0 && dir.z<0)
        ||  (dir.y<0 && dir.x>0 && dir.z<0)
        )
        matDiffuse = color2;}
else
{
    matDiffuse = vec4(abs(dir.xyz) * 0.8, 1.0);
}

