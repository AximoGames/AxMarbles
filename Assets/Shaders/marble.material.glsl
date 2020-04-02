matDiffuse = texture(material.DiffuseMap, TexCoords).rgb * material.DiffuseColor;
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
    matDiffuse = abs(dir.xyz) * 0.8;
}

