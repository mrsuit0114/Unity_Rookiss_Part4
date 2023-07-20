START ../../bin/Debug/PacketGenerator.exe ../../PDL.xml
XCOPY /Y GenPackets.cs "../../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../../Server/Packet"