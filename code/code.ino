#include<Wire.h>
#include <Servo.h>
#include "MS5837.h"

MS5837 sensor;
Servo servoCW;
Servo servoCCW;

bool verticalMode = true;
bool puttedValue = false;
int requiredDepth;

byte servoPinCW = 6;
byte servoPinCCW = 9;
int signalCW = 0;
int signalCCW = 0;

void setup(){
  Serial.begin(9600);
  Wire.begin();
  servoCW.attach(servoPinCW);
  servoCW.writeMicroseconds(1500);
  delay(7000);
  servoCCW.attach(servoPinCCW);
  servoCCW.writeMicroseconds(1500);
  delay(7000);
  
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
    sensor.read();
    Serial.print("Enter required depth: ");
    while(Serial.available()== 0  && puttedValue == false){
      // Do Nothing
      Serial.print("You haven't entered value yet");
    }
    requiredDepth = Serial.parseInt();
    puttedValue = true;
    if(sensor.depth()<=requiredDepth){
      // 256/2*0.5 = 64 for 0.5 meter
      // motor will rotate so that it will float at 0.5m.
      signalCW = 1718;
      signalCCW = 1246;
      servoCW.writeMicroseconds(signalCW);
      servoCCW.writeMicroseconds(signalCCW);
      Serial.print("Raw Value : ");
      Serial.print((float)sensor.depth());
      Serial.print(" meter\t");
      Serial.print("Output : ");
      Serial.print("CW-1718 And CCW-1246");
      Serial.print("\n");
    }else{
      // motor will rotate to go up
      signalCW = 1850;
      signalCCW = 1150;
      servoCW.writeMicroseconds(signalCW);
      servoCCW.writeMicroseconds(signalCCW);
      Serial.print("Raw Value : ");
      Serial.print((float)sensor.depth());
      Serial.print(" meter\t");
      Serial.print("Output : ");
      Serial.print("CW-1850 And CCW-1150");
      Serial.print("\n");
    }
    delay(1000);
  }else{
    // Vertical mode false
    // Write code for forward-backward movement
  } 
}
