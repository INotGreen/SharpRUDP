# Preface

I once attempted to develop RUDP as a non-standard TCP/UDP communication protocol, simultaneously possessing the reliability of TCP and the stealthiness of UDP. Its traffic might not be monitored by traditional security equipment. This could be something red teams and security developers are looking for. However, this is just an example. If you wish to incorporate it into a production application, you might still need to implement encryption algorithms and serialized transmission, among other operations.

# Communication Mechanism

#### 1. Three-Way Handshake:

- **SYN (Client -> Server)**: The client sends a packet with a SYN flag to request a connection.
- **SYN-ACK (Server -> Client)**: The server acknowledges the SYN and responds with a packet carrying both SYN and ACK flags.
- **ACK (Client -> Server)**: The client sends a packet with an ACK flag, confirming the connection is established.

#### 2. Data Transmission:

- The client and server can begin data exchange. After each data transfer, the recipient sends an ACK to confirm data reception. Similar to TCP, RUDP might acknowledge received packets, ensuring their successful arrival at the destination.

#### 3. Heartbeat Mechanism:

- To maintain the connection’s active status, both client and server periodically send heartbeat packets after the connection is established. If either party doesn’t receive a heartbeat within a specified period, it considers the connection broken and attempts reconnection.

#### 4. Packet Retransmission:

- A Boolean value 'IsConnected' is established. If the sender doesn’t receive an acknowledgment within a set period, it might retransmit the packet. Generally, the client sends an ACK-SYN packet to the server for packet handshake, verifying connection success. If connected, 'IsConnected' returns True; otherwise, it returns False, prompting reconnection attempts in a cycle.

#### 5. Orderliness and Security:

- **Encryption Techniques**: Utilize AES, RSA, or other encryption algorithms for RUDP data encryption. Encryption keys can be exchanged through the Diffie-Hellman key exchange protocol or other secure methods.
- **Identity Verification Mechanism**: Employ digital certificates and Public Key Infrastructure (PKI) to offer identity verification for RUDP connections, ensuring data authenticity and integrity while preventing middleman attacks.

#### 6. Congestion Control:

- RUDP needs to modify its packet sending rate to circumvent network congestion.

#### 7. Flow Control:

- RUDP employs a sliding window or other mechanisms to manage data flow, ensuring the recipient isn’t overwhelmed.

#### 8. Terminating Connection:

- When the client or server intends to end the connection, it sends a packet with a FIN flag. The recipient, upon receiving this packet, sends an ACK for confirmation and terminates the connection.

![image](https://github.com/INotGreen/SharpRUDP/assets/89376703/7442194b-28df-43b1-b7a2-f879c38859d9)
