from pythonosc import udp_client

client = udp_client.SimpleUDPClient("127.0.0.1", 1337)
client.send_message("/pressure", 123)