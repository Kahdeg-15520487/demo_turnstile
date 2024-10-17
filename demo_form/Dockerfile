# Use the official Node.js image as the base image
FROM node:16-alpine AS build

# Set the working directory
WORKDIR /app

# Copy package.json and package-lock.json
COPY package.json package-lock.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code
COPY . .

# Build the Angular application
RUN npm run build

# Use the official Nginx image as the base image for serving the application
FROM nginx:alpine

# Copy the built Angular application from the build stage
COPY --from=build /app/dist/angular.io-example /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start Nginx server
CMD ["nginx", "-g", "daemon off;"]