#version 330 core

out vec4 FragColor;

in vec3 TexCoord;

//struct WindowLevel {
//	float ww;
//	float wl;
//};
//
uniform isampler3D u_texture;
uniform float minPeak;
uniform float maxPeak;
//uniform WindowLevel winLevel;
uniform float windowWidth;
uniform float windowLevel;

float normalizeHistogram(float currentPixel, float minPeak, float maxPeak, float newMin, float newMax) {
	return newMin + (currentPixel - minPeak) / (maxPeak - minPeak) * (newMax-newMin);
}

float applyWindowLevel(float currentPixel, float ww, float wl, float newMin, float newMax) {
	float minVal = wl - 0.5 * ww;
	float maxVal = wl + 0.5 * ww;

	if (currentPixel <= minVal) { 
		return newMin;
	}
	else if (currentPixel > maxVal) { 
		return newMax; 
	} 
	else {
		return newMin + (currentPixel - wl + 0.5 * ww) * (newMax - newMin) / ww; 
	}

}



void main() {
	int texValue = texture(u_texture, TexCoord).r;
	float newValue = normalizeHistogram(float(texValue), minPeak, maxPeak, 0.0, 1.0);
//	float  newValue = applyWindowLevel(float(texValue), windowWidth, windowLevel, 0.0, 1.0);
	FragColor = vec4(newValue,newValue,newValue, 1);
}

