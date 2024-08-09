#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/asio.hpp>
#include <iostream>

namespace beast = boost::beast;
namespace http = beast::http;
namespace net = boost::asio;
using tcp = net::ip::tcp;

void handle_request(const http::request<http::string_body>& req, http::response<http::string_body>& res) {
    res.version(11);
    res.result(http::status::ok);
    res.set(http::field::content_type, "text/plain");
    res.body() = "Hello, World!";
    res.prepare_payload();
}

int main() {
    try {
        net::io_context ioc{1};
        tcp::acceptor acceptor{ioc, {tcp::v4(), 8080}};

        for (;;) {
            tcp::socket socket{ioc};
            acceptor.accept(socket);
            
            beast::flat_buffer buffer;
            http::request<http::string_body> req;
            http::read(socket, buffer, req);
            
            http::response<http::string_body> res;
            handle_request(req, res);
            http::write(socket, res);
            
            socket.shutdown(tcp::socket::shutdown_send);
        }
    } catch (const std::exception& e) {
        std::cerr << "Error: " << e.what() << std::endl;
    }
    return 0;
}

