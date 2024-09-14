# Architecture Diagram

![image](https://github.com/user-attachments/assets/4d3cd318-10c8-4237-9316-0b063007b8ae)

# Component Description

- User: A Person who use the Quizz app
- AWS Route 53: Specific DNS (Domain name service) for Quizz app which help routing user's requests to the application
- AWS CloudFront: Setup CDN to help quickly delivers data to users, this one fit with the requirement of an real-time Quizz application where latency need to reduce to maximum
- AWS API Gateway: API Gateway help specific user's request types (REST/Websocket) and help routing user's requests to correct services
- AWS Cognito: A service help authorize user's request which enhance security of the application
- AWS Elastic Load Balancing: Automatic distributes user's requests across multiple services instances. This helps enhance the availability and fault tolerance of the application
- AWS auto-scale EC2 instances: EC2 instances where application's services are hosted and running
- Lambda functions: Run some custom functions like sent emails (when users registration or forgot password) etc
- AWS RDS MSSQL instance: Store the application data
- Redis cache: Caching data to reduce the request to database, this will help reduce user's requests process times
- Log and Monitor with Datadog AWS integration: Datadog service help monitor services health, metrics and services logging in real-time 

# Data Flow

![image](https://github.com/user-attachments/assets/a953adf2-bfe1-420f-89c7-8285a4b8929d)

# Technologies and Tools

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

  
