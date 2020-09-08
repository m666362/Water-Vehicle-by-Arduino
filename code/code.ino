#include<Wire.h>
#include "MS5837.h"

MS5837 sensor;

bool verticalMode = true;
// int requiredDepth = 127;
// int currentDepth = 0;
int sensorValue = 0;
int outputValue = 0;
int waterHeight = 2;

const int verticalThrusterCW = 6;
const int verticalThrusterCCW = 9;
const int sensorPin = A0;

void setup(){
  Serial.begin(9600);
  Wire.begin();
  Serial.print("Starting Machine");
  while(! sensor.init()){
    Serial.print("Starting Failed!\nCheck connection");
    delay(5000);
  }
  sensor.setModel(MS5837::MS5837_30BA);
  sensor.setFluidDensity(997); // kg/m^3 (freshwater, 1029 for seawater)
}
void loop(){
  /*
    if(modeChangeButton == HIGH){
      verticalMode = true;
    }else{
      verticalMode = false;
    }
  */
  if(verticalMode == true){
    // Write code for up-down movement
    // sensorValue = analogRead(sensorPin);
    // outputValue = map(sensorValue, 0, 1023, 0, 255);
    sensor.read();
    if(sensor.depth()<=0.5){
      // 256/2*0.5 = 64 for 0.5 meter
      // motor will rotate so that it will float at 0.5m.
      analogWrite(verticalThrusterCW, 116);
      analogWrite(verticalThrusterCCW, 116);
      Serial.print("Raw Value : ");
      Serial.print((float)sensor.depth());
      Serial.print(" meter\t");
      // Serial.print("Input : ");
      // Serial.print(sensorValue);
      // Serial.print("\t");
      Serial.print("Output : ");
      Serial.print("116");
      Serial.print("\n");
    }else{
      // motor will rotate to go up
      // I have given 220 instead of 255 because i want to move robot slowly
      analogWrite(verticalThrusterCW, 220);
      analogWrite(verticalThrusterCCW, 220);
      Serial.print("Raw Value : ");
      Serial.print((float)sensor.depth());
      Serial.print(" meter\t");
      // Serial.print("Input : ");
      // Serial.print(sensorValue);
      // Serial.print("\t");
      Serial.print("Output : ");
      Serial.print("220");
      Serial.print("\n");
    }
    delay(1000);
  }else{
    // Vertical mode false
    // Write code for forward-backward movement
  } 
}
