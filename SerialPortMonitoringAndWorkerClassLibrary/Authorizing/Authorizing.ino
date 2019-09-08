#include <SPI.h>
#include <MFRC522.h>

#define RST_PIN 9
#define SS_PIN 10

String MasterTag = "XXXXXXXX";
String StringPrefix = "Authy: ";

void setup() 
{
  Serial.begin(9600);
  SPI.begin();
}

void loop() 
{
  delay(2000);
  
  // Create instances
  MFRC522 mfrc522(SS_PIN, RST_PIN);
  mfrc522.PCD_Init(); // MFRC522
  
  //If a new PICC placed to RFID reader continue
  if ( ! mfrc522.PICC_IsNewCardPresent()) 
  { 
    ConsolePrint(" Access  Denied! (NewCardPresent)");
    return;
  }
  
  //Since a PICC placed get Serial and continue
  if ( ! mfrc522.PICC_ReadCardSerial()) 
  { 
    ConsolePrint(" Access Denied! (ReadCardSerial)");
    return;
  }
  
  String tagID = "";
  for ( uint8_t i = 0; i < 4; i++) // The MIFARE PICCs that we use have 4 byte UID
  { 
    tagID.concat(String(mfrc522.uid.uidByte[i], HEX)); // Adds the 4 bytes in a single String variable
  }
  
  mfrc522.PICC_HaltA(); // Stop reading

  tagID.toUpperCase();
  if (tagID == MasterTag) 
  {
    ConsolePrint(" Access Granted!");
  }
  else
  {
    ConsolePrint(" Access Denied!");
  }
}

void ConsolePrint(String text)
{
  Serial.println(StringPrefix + text);
}
