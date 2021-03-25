from pythonosc import udp_client
from random import random
import time

client = udp_client.SimpleUDPClient("127.0.0.1", 1337)
while True:
  client.send_message("/pillar_hit", random())
  time.sleep(random()*2)