#include <math.h>
#include "Vect.hpp"
#include <fstream>

const double pi = 3.1415926;

void Vect::insert(double x)
{
	data.push_back(x);
	size++;
	return;
}
double Vect::length()
{
	double sum = 0;
	for(int i = 0; i < size; i++)
	{
		sum = sum + pow(data[i], 2);
	}
	return sqrt(sum);
}

Vect Vect::operator-(Vect other)
{
	vector<double> dif = vector<double>(size, 0);
	for(int i = 0; i < size; i++)
	{
		dif[i] = data[i] - other.data[i];
	}
	return Vect(dif);
}

double Vect::operator*(Vect other)
{
	double s = 0;
	for(int i = 0; i < size; i++)
	{
		s = s + data[i] * other.data[i];
	}
	return s;
}

void Vect::disp(ofstream& fid)
{
	for(int k = 0; k < size; k++)
	{
		fid << data[k] << " ";
	}
	return;
}

double dist(Vect one, Vect other)
{
	Vect d = one - other;
	return d.length();
}

double angle(Vect one, Vect other)
{
	double a = acos(one*other/(one.length()*other.length()))/pi*180;
	return a;
}

Vect cross(Vect one, Vect other)
{
	if (one.size != 3 || other.size != 3)
		return Vect(0.0, 0.0, 0.0);
	double x1 = one.data[0];
	double y1 = one.data[1];
	double z1 = one.data[2];

	double x2 = other.data[0];
	double y2 = other.data[1];
	double z2 = other.data[2];

	double x = y1*z2 - y2*z1;
	double y = x2*z1 - x1*z2;
	double z = x1*y2 - x2*y1;

	return Vect(x, y, z);
}