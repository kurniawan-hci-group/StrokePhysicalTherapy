#ifndef VECT_HPP
#define VECT_HPP

#include <vector>

using namespace std;

class Vect{      //vector
public:
	int size;
	vector<double> data;

	// construction
	Vect(vector<double> data1): size(data1.size()), data(data1) {}
	Vect(int size1, vector<double> data1): size(size1), data(data1) {} 
	Vect(int size1)
	{
		size = size1;
		data = vector<double>(size, 0.0);
	}
	Vect()
	{
		size = 0;
		data = vector<double>(size, 0.0);
	}
	Vect(double x, double y, double z)  // normal case
	{
		size = 3;
		data.push_back(x);
		data.push_back(y);
		data.push_back(z);
	}
	Vect(double x1, double x2, double x3, double x4)
	{
		size = 4;
		data.push_back(x1);
		data.push_back(x2);
		data.push_back(x3);
		data.push_back(x4);
	}
	Vect(double x1, double x2, double x3, double x4, double x5)
	{
		size = 5;
		data.push_back(x1);
		data.push_back(x2);
		data.push_back(x3);
		data.push_back(x4);
		data.push_back(x5);
	}
	Vect(double x1, double x2, double x3, double x4, double x5, double x6)
	{
		size = 6;
		data.push_back(x1);
		data.push_back(x2);
		data.push_back(x3);
		data.push_back(x4);
		data.push_back(x5);
		data.push_back(x6);
	}
	Vect(double x1, double x2, double x3, double x4, double x5, double x6, double x7)
	{
		size = 7;
		data.push_back(x1);
		data.push_back(x2);
		data.push_back(x3);
		data.push_back(x4);
		data.push_back(x5);
		data.push_back(x6);
		data.push_back(x7);
	}

	void insert(double x);
	double length();

	Vect operator-(Vect other);
	double operator*(Vect other);  // inner product
	void disp(ofstream& fid);
	
};

double dist(Vect one, Vect other);
double angle(Vect one, Vect other);
Vect cross(Vect one, Vect other);

#endif