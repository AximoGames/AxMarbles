// --- LIB ---

// --- CALL ---

color = BlendColor(texture(material.diffuse, TexCoords).rgb, material.color, material.colorBlendMode);

if((Normal.y>0 && Normal.x>0) || (Normal.y<0 && Normal.x<0))
    color *= 0.5;