package com.prueba.feeservice.config;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Info;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class OpenApiConfig {

    @Bean
    public OpenAPI feeServiceOpenApi() {
        return new OpenAPI().info(new Info()
                .title("Fee Service API")
                .version("v1")
                .description("Servicio de cálculo de comisiones por canal (WEB, MOBILE, BRANCH)."));
    }
}