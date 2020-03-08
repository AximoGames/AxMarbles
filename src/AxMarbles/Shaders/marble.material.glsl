color = BlendColor(texture(material.diffuse, TexCoords).rgb, material.color, material.colorBlendMode);

if(joker==0.0)
{
    if(
        (Normal.y>0 && Normal.x>0 && Normal.z>0)
        || (Normal.y<0 && Normal.x<0 && Normal.z>0)
        ||  (Normal.y>0 && Normal.x<0 && Normal.z<0)
        ||  (Normal.y<0 && Normal.x>0 && Normal.z<0)
        )
        color = color2;}
else
{
    color = abs(Normal.xyz) * 0.8;
}

