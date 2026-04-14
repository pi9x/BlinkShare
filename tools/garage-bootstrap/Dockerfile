FROM dxflrs/garage:v2.2.0 AS garage

FROM alpine:3.20

RUN apk add --no-cache bash grep

COPY --from=garage /garage /usr/local/bin/garage
COPY tools/garage-bootstrap/bootstrap.sh /usr/local/bin/bootstrap-garage

RUN chmod +x /usr/local/bin/bootstrap-garage

ENTRYPOINT ["/usr/local/bin/bootstrap-garage"]
