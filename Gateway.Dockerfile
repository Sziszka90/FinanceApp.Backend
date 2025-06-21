# NGINX Gateway Dockerfile for Azure Containerized App
FROM nginx:1.27-alpine

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx config
COPY gateway-nginx.conf /etc/nginx/conf.d/

# Expose the gateway port
EXPOSE 8081

CMD ["nginx", "-g", "daemon off;"]
