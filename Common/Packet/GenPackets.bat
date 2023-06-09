START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../Inflearn_DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../Inflearn_Server/Packet"