// Please see the definition of Vect in Vect.hpp and Vect.cpp

// calculate elbow/knee angles
double RightElbowAngle()
{
	Vect v1 = frame[8] - frame[9];
	Vect v2 = frame[10] - frame[9];
	return angle(v1, v2);
}

// calculate trunk angle with the vertical direction
double TrunkAngle()
{
	Vect v1 = frame[20] - frame[0];  // trunk
	Vect v2 = Vect(0.0, -1.0, 0.0);  // the vertical direction may need calibration
	return angle(v1, v2);
}

// calculate the leg direction
// when the user is standing, the angle can't represent the direction
double RightLegDirection()   // -180 ~ 180, if nearly upright, return 200
{
	Vect v1 = frame[18] - frame[16];  // right leg
	double x = v1.data[0];
	double z = v1.data[2];
	Vect p = Vect(x, 0.0, z);  // projection on the ground (x-z plane)

	// if nearly upright (the user is standing), return 200
	double a = angle(p, v1);  // 0 ~ 90
	if (a > 85)
		return 200;

	//Vect v2 = Vect(1.0, 0.0, 0.0);  // v2 points to the right (but may need calibration)
	Vect shoulder = frame[8] - frame[4];
	Vect v2 = Vect(shoulder.data[0], 0.0, shoulder.data[2]);

	if (x*shoulder.data[2] - z*shoulder.data[0] >= 0)  // v2 x p is upward, so v2 rotates counter clockwise to p
		return angle(p, v2);
	else
		return -angle(p, v2);
}