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
# -------------------------------------------------------------------
def open_file(filename):
  with open(filename, 'r') as myfile:
      data=myfile.read()
  return json.loads(data)['soles']

# -------------------------------------------------------------------
# convert the JSON data into something a bit more python-y
# -------------------------------------------------------------------
def pythonify_data(data):
  for row in data:
    day = {
      "min_temp": row["min_temp"],
      "max_temp": row["max_temp"],
      "pressure": row["pressure"],
      "radiation": row["local_uv_irradiance_index"]
    }
    weather_array.append(day)

# -------------------------------------------------------------------
# add normalised data to weather_data array
# -------------------------------------------------------------------
def normalise_data():
  i = 0 # keep track of where we are in weather array
  min_pressure = find_min('pressure')
  max_pressure = find_max('pressure')
  min_min_temp = find_min('min_temp')
  max_min_temp = find_max('min_temp')
  min_max_temp = find_min('max_temp')
  max_max_temp = find_max('max_temp')

  for row in weather_array:
    # normalise temp
    row['min_temp_norm'] = norm(row['min_temp'], min_min_temp, max_min_temp)
    row['max_temp_norm'] = norm(row['max_temp'], min_max_temp, max_max_temp)

    # normalise pressure
    row['pressure_norm'] = norm(row['pressure'], min_pressure, max_pressure)

    # normalise_radiation
    if row['radiation'] == 'Low':
      row["radiation_norm"] = 1
    elif row['radiation'] == 'Moderate':
      row["radiation_norm"] = 2
    elif row['radiation'] == 'High':
      row["radiation_norm"] = 3
    elif row['radiation'] == 'Very_High':
      row['radiation_norm'] = 4
    else:
      row['radiation_norm'] = '--'

    # combined weather - cold, moderate, hot
    row['weather_norm'] = calc_weather(row)

    weather_array[i] = row
    i+=1

# -------------------------------------------------------------------
# calculate combined weather
# -------------------------------------------------------------------
def calc_weather(row):
  if row['max_temp_norm'] == '--': return '--'
  if row['pressure_norm'] == '--': return '--'

  combined_val = (row['max_temp_norm'] + row['pressure_norm']) / 2
  if 0 <= round(combined_val, 2) <= 0.5:
    return 1 # cold
  elif 0.51 <= round(combined_val, 2) <= 0.6:
    return 2 # moderate
  elif 0.61 <= round(combined_val, 2) <= 1:
    return 3 # hot

# -------------------------------------------------------------------
# calculate normalised value given the min and max
# -------------------------------------------------------------------
def norm(val, min_val, max_val):
  if val != '--':
    return round((int(val)-min_val)/(max_val-min_val), 2)
  else:
    return '--'

# -------------------------------------------------------------------
# find max value in given weather data column
# -------------------------------------------------------------------
def find_max(column):
  max_val = 0
  for row in weather_array:
    if row[column] == '--': continue
    cur_val = int(row[column])
    if cur_val > max_val: max_val = cur_val
  return max_val

def find_min(column):
  min_val = 10000000
  for row in weather_array:
    if row[column] == '--': continue
    cur_val = int(row[column])
    if cur_val < min_val: min_val = cur_val
  return min_val

# -------------------------------------------------------------------
# sending values to Max and Unity
# -------------------------------------------------------------------
def send_to(client, index):
  client.send_message("/min_temp", fetch_data(index, 'min_temp'))
  client.send_message("/max_temp", fetch_data(index, 'max_temp'))
  client.send_message("/pressure", fetch_data(index, 'pressure'))
  client.send_message("/radiation",fetch_data(index, 'radiation'))
  client.send_message("/min_temp_norm", fetch_data(index, 'min_temp_norm'))
  client.send_message("/max_temp_norm", fetch_data(index, 'max_temp_norm'))
  client.send_message("/pressure_norm", fetch_data(index, 'pressure_norm'))
  client.send_message("/radiation_norm",fetch_data(index, 'radiation_norm'))
  client.send_message("/weather_norm",  fetch_data(index, 'weather_norm'))

def fetch_data(index, column):
  val = last_valid_value(index, column)
  if column == 'pressure' or column == 'pressure_norm':
    val = val * pressure_adjustment
    if column == 'pressure_norm':
      val = 1 if val > 1 else 0 if val < 0 else val
  if column == 'min_temp' or column == 'min_temp_norm':
    val = val * temp_adjustment
    if column == 'min_temp_norm':
      val = 1 if val > 1 else 0 if val < 0 else val
  if column == 'max_temp' or column == 'max_temp_norm':
    val = val * temp_adjustment
    if column == 'max_temp_norm':
      val = 1 if val > 1 else 0 if val < 0 else val
  return val

# -------------------------------------------------------------------
# recursive function to find the previous non-null value
# -------------------------------------------------------------------
def last_valid_value(index, column):
  value = weather_array[index][column]
  if value != '--':
    return value
  else:
    return last_valid_value(index-1, column)

# -------------------------------------------------------------------
# set up the endpoints that the OSC server will expose
# -------------------------------------------------------------------
def configure_dispatcher():
  dispatcher.map("/temp", receive_temp)
  dispatcher.map("/pressure", receive_pressure)
  dispatcher.map("/radiation", receive_radiation)

# -------------------------------------------------------------------
# The following are the handlers for the endpoints
# -------------------------------------------------------------------
def receive_temp(address: str, *args: List[Any]) -> None:
  temp = args[0]
  print(f"received temp adjustment {temp}")
  global temp_adjustment
  temp_adjustment += temp
  print(f"temp_adjustment is now {temp_adjustment}")
# -------------------------------------------------------------------
def receive_pressure(address: str, *args: List[Any]) -> None:
  pressure = args[0]
  print(f"received pressure adjustment {pressure}")
  global pressure_adjustment
  pressure_adjustment += pressure
  print(f"pressure_adjustment is now {pressure_adjustment}")
# -------------------------------------------------------------------
def receive_radiation(address: str, *args: List[Any]) -> None:
  radiation = args[0]
  print(f"received radiation adjustment {radiation}")
  # TODO: this is a text field - how to adjust this?
  # radiation_adjustment += radiation

# -------------------------------------------------------------------
# run the OSC server its own thread
# -------------------------------------------------------------------
def start_osc_server():
  server = osc_server.ThreadingOSCUDPServer(
                      ("127.0.0.1", 1337),
                      dispatcher)
  print("Serving on {}".format(server.server_address))
  server.serve_forever()


# -------------------------------------------------------------------
# MAIN
# -------------------------------------------------------------------
if __name__ == "__main__":
  # load and parse the weather data
  json_data = open_file('rems_weather_data.json')
  pythonify_data(json_data)
  normalise_data()
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
    send_to(max_client, i)
    send_to(unity_client, i)
    time.sleep(0.1)
    i+=1
    # if we reach the end, go back to the beginning!
    if i == len(weather_array): i = 0



