# -------------------------------------------------------------------
# Mars Weather Processor
# This script extracts weather data from a JSON file downloaded from
# https://mars.nasa.gov/msl/weather/ and shares this data with
# enpoints exposed by Max MSP and the Unity engine.
# It also acts as an OSC server, allowing two-way communication with
# Max and Unity.
# The server runs in its own thread, which allows us to continuously
# send weather data to Unity and Max at the same time.
# -------------------------------------------------------------------
import json
import time
import threading
from typing import List, Any

from pythonosc.dispatcher import Dispatcher
from pythonosc import udp_client
from pythonosc import osc_server

# -------------------------------------------------------------------
# GLOBALS
# -------------------------------------------------------------------
dispatcher = Dispatcher()
weather_array = [] # for the parsed weather data
# adjustment values received from Unity:
temp_adjustment = 1
pressure_adjustment = 1
radiation_adjustment = 1

# -------------------------------------------------------------------
# FUNCTIONS
# -------------------------------------------------------------------
# read in the JSON data
def open_file(filename):
  with open(filename, 'r') as myfile:
      data=myfile.read()
  return json.loads(data)['soles']

# convert the JSON data into something a bit more python-y
def pythonify_data(data):
  for row in data:
    day = {
      "min_temp": row["min_temp"],
      "max_temp": row["max_temp"],
      "pressure": row["pressure"],
      "radiation": row["local_uv_irradiance_index"]
    }
    weather_array.append(day)

# sending values to Max and Unity
def send_to(client, data):
  client.send_message("/min_temp",  data['min_temp']  * temp_adjustment)
  client.send_message("/max_temp",  data['max_temp']  * temp_adjustment)
  client.send_message("/pressure",  data['pressure']  * pressure_adjustment)
  client.send_message("/radiation", data['radiation'])

# set up the endpoints that the OSC server will expose
def configure_dispatcher():
  dispatcher.map("/temp", receive_temp)
  dispatcher.map("/pressure", receive_pressure)
  dispatcher.map("/radiation", receive_radiation)

# The following are the handlers for the 3 endpoints
def receive_temp(address: str, *args: List[Any]) -> None:
  temp = args[0]
  print(f"received temp adjustment {temp}")
  global temp_adjustment
  temp_adjustment += temp
  print(f"temp_adjustment is now {temp_adjustment}")

def receive_pressure(address: str, *args: List[Any]) -> None:
  pressure = args[0]
  print(f"received pressure adjustment {pressure}")
  global pressure_adjustment
  pressure_adjustment += pressure
  print(f"pressure_adjustment is now {pressure_adjustment}")

def receive_radiation(address: str, *args: List[Any]) -> None:
  radiation = args[0]
  print(f"received radiation adjustment {radiation}")
  # TODO: this is a text field - how to adjust this?
  # radiation_adjustment += radiation

# run the OSC server its own thread
def start_osc_server():
  server = osc_server.ThreadingOSCUDPServer(
                      ("127.0.0.1", 1337),
                      dispatcher)
  print("Serving on {}".format(server.server_address))
  server.serve_forever()

# -------------------------------------------------------------------


if __name__ == "__main__":
  # load and parse the weather data
  json_data = open_file('rems_weather_data.json')
  pythonify_data(json_data)

  # -----------------------------------------------------------------
  # OSC server setup
  # -----------------------------------------------------------------
  # set our OSC endpoints
  configure_dispatcher()
  # add the OSC server to a thread
  server = threading.Thread(target=start_osc_server)
  # start the server
  server.start()

  # -----------------------------------------------------------------
  # OSC client setup
  # -----------------------------------------------------------------
  # create OSC clients - one to connect to MAX, the other to Unity
  max_client   = udp_client.SimpleUDPClient('127.0.0.1', 4444)
  unity_client = udp_client.SimpleUDPClient('127.0.0.1', 4445)

  # the following sets up a repeating loop through the weather data
  # each iteration sends the weather data row to Max and Unity
  i = 0
  while i < len(weather_array):
    row = weather_array[i]
    print(f"sending row: {row}")
    send_to(max_client, row)
    send_to(unity_client, row)
    time.sleep(1)
    i+=1
      # if we reach the end, go back to the beginning!
    if i == len(weather_array): i = 0



