
  

# Event-Driven Microservices Application

  

This project is an example of an event-driven application with microservices, developed using .NET 8 and Docker. The application is designed to handle user creation events and order processing, with RabbitMQ managing the event messaging between services.

  

## Overview

  

The application consists of three core microservices:

  

1.  **UserService**: Handles user creation and management.

2.  **OrderService**: Manages order creation and processing.

3.  **NotificationService**: Sends notifications and manages user and order events.

  

### Event-Driven Architecture

  

-  **User Creation Event**: When a user is created via the `UserService`, an event (`user.created`) is published to RabbitMQ. This event is consumed by both the `NotificationService` and the `OrderService`.

-  **NotificationService**: The `NotificationService` subscribes to the `user.created` event. When it receives this event, it stores the user's information and sends a welcome email to the user.

-  **OrderService**: The `OrderService` also subscribes to the `user.created` event. It stores the user ID in its own database to ensure referential integrity when an order is made.

  

-  **Order Creation Event**: When an order is created via the `OrderService`, an event (`order.created`) is published to RabbitMQ. The `NotificationService` subscribes to this event and sends an email with the order details to the user.

  

### Microservices

  

Each microservice is independently deployable and runs in its own Docker container. The services communicate with each other asynchronously through RabbitMQ.

  

-  **Database**: PostgreSQL is used as the database for all three services, with separate databases for each service.

-  **RabbitMQ**: Manages the messaging between services, ensuring that events are properly routed to the subscribing services.

-  **SMTP4Dev**: Used for testing email notifications sent by the `NotificationService`.

  

## Docker Setup

  

The project uses Docker Compose to define and run multi-container Docker applications. The `docker-compose.yml` file defines the services and their configurations.

  

## Running the Application

  

To run the application, use the following command:

  

```sh

docker-compose  up  --build

```

  

This will start all the services defined in the `docker-compose.yml` file.

  

## Conclusion

  

This project demonstrates how to build a scalable, event-driven architecture using .NET 8 and Docker. The microservices communicate asynchronously using RabbitMQ, allowing each service to operate independently while still being part of a cohesive system. The `NotificationService` enhances user engagement by sending welcome emails upon user registration and order confirmation emails when an order is placed.

  

Feel free to explore, modify, and expand upon this example to suit your own needs!