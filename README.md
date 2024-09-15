# Architecture Diagram

![image](https://github.com/user-attachments/assets/aead38c7-7b0a-40ea-ba86-5a06b8e25564)

# Component Description

- User: A Person who use the Quizz app
- AWS Route 53: Specific DNS (Domain name service) for Quizz app which help routing user's requests to the application
- AWS CloudFront: Setup CDN to help quickly delivers data to users, this one fit with the requirement of an real-time Quizz application where latency need to reduce to maximum
- AWS API Gateway: API Gateway help specific user's request types (REST/Websocket) and help routing user's requests to correct services
- AWS Cognito: A service help authorize user's request which enhance security of the application
- AWS Elastic Load Balancing: Automatic distributes user's requests across multiple services instances. This helps enhance the availability and fault tolerance of the application
- AWS auto-scale EC2 instances: EC2 instances where application's services are hosted and running
- SNS service for push notification: This important service will help update the user answers, points and notification message in real-time during Quizz session. The service will support Websocket protocol to keep update whole Quizz contents in real-time
- Lambda functions: Run some custom functions like sent emails (when users registration or forgot password) etc
- AWS RDS MSSQL instance: Store the application data
- Redis cache: Caching data to reduce the request to database, this will help reduce user's requests process times
- Log and Monitor with Datadog AWS integration: Datadog service help monitor services health, metrics and services logging in real-time 

# Data Flow

![image](https://github.com/user-attachments/assets/0d948dbe-78d4-41da-bbd3-afadf9c5882d)

# Technologies and Tools
This project will be implement in .Net framework and C# using Visual Studio v19. The SNS service will be implement using SignalR which support Websocket protocol to update Quizz contents in real-time, SignalR also compatible with .Net framework and easy to setup.
- AWS Route 53: Specific DNS (Domain name service) for Quizz app
- AWS CloudFront: Setup CDN to help quickly delivers data to users, this one fit with the requirement of an real-time Quizz application where latency need to reduce to maximum
- AWS API Gateway: API Gateway help specific user's request types (REST/Websocket) and help routing user's requests to correct services
- SignalR: Support Websocket API which will deliver data in real-time
- AWS Cognito: A service help authorize user's request which enhance security of the application
- AWS Elastic Load Balancing: Automatic distributes user's requests across multiple services instances. This helps enhance the availability and fault tolerance of the application
- AWS auto-scale EC2 instances: EC2 instances where application's services are hosted and running
- Lambda functions: Run some custom functions like sent emails (when users registration or forgot password) etc
- AWS RDS MSSQL instance: Store the application data
- Redis cache: Caching data to reduce the request to database, this will help reduce user's requests process times
- Log and Monitor with Datadog AWS integration: Datadog service help monitor services health, metrics and services logging in real-time

  
