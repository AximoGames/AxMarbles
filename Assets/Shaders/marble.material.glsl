matDiffuse = BlendColor(texture(material.DiffuseMap, TexCoords).rgb, material.DiffuseColor, material.ColorBlendMode);

if(joker == 0.0)
{
    if(
        (Normal.y>0 && Normal.x>0 && Normal.z>0)
        || (Normal.y<0 && Normal.x<0 && Normal.z>0)
        ||  (Normal.y>0 && Normal.x<0 && Normal.z<0)
        ||  (Normal.y<0 && Normal.x>0 && Normal.z<0)
        )
        matDiffuse = color2;}
else
{
    matDiffuse = abs(Normal.xyz) * 0.8;
}

