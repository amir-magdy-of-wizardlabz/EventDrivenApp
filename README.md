# Event-Driven Microservices Architecture

This repository contains the setup for an event-driven microservices architecture using Docker Compose. The services included in this architecture are:

- **PostgreSQL Database** for data persistence.
- **RabbitMQ** for messaging between services.
- **SMTP4Dev** for email testing.
- **UserService API** for managing users.
- **OrderService API** for managing orders.
- **NotificationService API** for handling notifications.

## Prerequisites

Ensure that you have Docker and Docker Compose installed on your machine.

## Running the Application

To start all services, run the following command in the root directory:

```bash
docker-compose up --build
