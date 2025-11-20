(function () {
    $(function () {
        var _dashboardService = abp.services.app.dashboard;
        
        var dashboardApp = {
            charts: {
                salesChart: null,
                categoryChart: null
            },
            
            // Load dashboard stats
            loadStats: function() {
                abp.ui.setBusy();
                _dashboardService.getDashboardStats()
                    .done(function(result) {
                        dashboardApp.updateStats(result);
                    })
                    .fail(function(error) {
                        abp.notify.error('Không thể tải thống kê dashboard');
                        console.error(error);
                    })
                    .always(function() {
                        abp.ui.clearBusy();
                    });
            },
            
            // Update stats cards
            updateStats: function(data) {
                // Main stats
                $('#totalRevenue').html(dashboardApp.formatCurrency(data.totalRevenue));
                $('#totalOrders').text(data.totalOrders);
                $('#totalProducts').text(data.totalProducts);
                $('#totalCustomers').text(data.totalCustomers);
                
                // Growth indicators
                var revenueGrowth = data.revenueGrowth.toFixed(1);
                var revenueIcon = data.revenueGrowth >= 0 ? 'fa-arrow-up' : 'fa-arrow-down';
                var revenueClass = data.revenueGrowth >= 0 ? 'text-white' : 'text-warning';
                $('#revenueGrowth').html(
                    '<i class="fas ' + revenueIcon + ' ' + revenueClass + '"></i> ' + 
                    Math.abs(revenueGrowth) + '% so với tháng trước'
                );
                
                var ordersGrowth = data.ordersGrowth;
                var ordersIcon = data.ordersGrowth >= 0 ? 'fa-arrow-up' : 'fa-arrow-down';
                var ordersClass = data.ordersGrowth >= 0 ? 'text-white' : 'text-warning';
                $('#ordersGrowth').html(
                    '<i class="fas ' + ordersIcon + ' ' + ordersClass + '"></i> ' + 
                    Math.abs(ordersGrowth) + '% so với tháng trước'
                );
                
                // Low stock info
                if (data.lowStockProducts > 0) {
                    $('#lowStockInfo').html(
                        '<i class="fas fa-exclamation-triangle"></i> ' + 
                        data.lowStockProducts + ' sản phẩm sắp hết'
                    );
                } else {
                    $('#lowStockInfo').html('<i class="fas fa-check"></i> Tồn kho ổn định');
                }
                
                // Average order value
                $('#avgOrderValue').html(
                    'Giá trị trung bình: ' + dashboardApp.formatCurrency(data.averageOrderValue)
                );
                
                // Additional stats
                $('#pendingOrders').text(data.pendingOrders);
                $('#completedOrders').text(data.completedOrders);
                $('#activeVouchers').text(data.activeVouchers);
                $('#lowStockProducts').text(data.lowStockProducts);
            },
            
            // Load sales chart
            loadSalesChart: function() {
                _dashboardService.getSalesChartData(30)
                    .done(function(result) {
                        dashboardApp.renderSalesChart(result);
                    })
                    .fail(function(error) {
                        console.error('Error loading sales chart:', error);
                    });
            },
            
            // Render sales chart
            renderSalesChart: function(data) {
                var ctx = document.getElementById('salesChart').getContext('2d');
                
                if (dashboardApp.charts.salesChart) {
                    dashboardApp.charts.salesChart.destroy();
                }
                
                dashboardApp.charts.salesChart = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: data.map(d => d.label),
                        datasets: [{
                            label: 'Doanh thu (VNĐ)',
                            data: data.map(d => d.value),
                            backgroundColor: 'rgba(54, 162, 235, 0.2)',
                            borderColor: 'rgba(54, 162, 235, 1)',
                            borderWidth: 2,
                            fill: true,
                            tension: 0.4
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: true,
                                position: 'top'
                            },
                            tooltip: {
                                callbacks: {
                                    label: function(context) {
                                        return 'Doanh thu: ' + dashboardApp.formatCurrency(context.parsed.y);
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    callback: function(value) {
                                        return dashboardApp.formatCurrency(value);
                                    }
                                }
                            }
                        }
                    }
                });
            },
            
            // Load category chart
            loadCategoryChart: function() {
                _dashboardService.getRevenueByCategory()
                    .done(function(result) {
                        dashboardApp.renderCategoryChart(result);
                    })
                    .fail(function(error) {
                        console.error('Error loading category chart:', error);
                    });
            },
            
            // Render category chart
            renderCategoryChart: function(data) {
                var ctx = document.getElementById('categoryChart').getContext('2d');
                
                if (dashboardApp.charts.categoryChart) {
                    dashboardApp.charts.categoryChart.destroy();
                }
                
                dashboardApp.charts.categoryChart = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: data.map(d => d.label),
                        datasets: [{
                            data: data.map(d => d.value),
                            backgroundColor: data.map(d => d.color),
                            borderWidth: 2,
                            borderColor: '#fff'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: true,
                                position: 'bottom'
                            },
                            tooltip: {
                                callbacks: {
                                    label: function(context) {
                                        return context.label + ': ' + dashboardApp.formatCurrency(context.parsed);
                                    }
                                }
                            }
                        }
                    }
                });
            },
            
            // Load top selling products
            loadTopProducts: function() {
                _dashboardService.getTopSellingProducts(5)
                    .done(function(result) {
                        dashboardApp.renderTopProducts(result);
                    })
                    .fail(function(error) {
                        console.error('Error loading top products:', error);
                        $('#topProductsTable').html(
                            '<tr><td colspan="3" class="text-center text-danger">Không thể tải dữ liệu</td></tr>'
                        );
                    });
            },
            
            // Render top products table
            renderTopProducts: function(products) {
                var html = '';
                
                if (products.length === 0) {
                    html = '<tr><td colspan="3" class="text-center text-muted">Chưa có dữ liệu</td></tr>';
                } else {
                    products.forEach(function(product, index) {
                        html += '<tr class="top-product-item">';
                        html += '<td>';
                        html += '<div class="d-flex align-items-center">';
                        html += '<img src="' + product.imageUrl + '" class="img-thumbnail mr-2" style="width: 50px; height: 50px; object-fit: cover;">';
                        html += '<div>';
                        html += '<div class="font-weight-bold">' + product.productName + '</div>';
                        html += '<small class="text-muted">Top ' + (index + 1) + '</small>';
                        html += '</div>';
                        html += '</div>';
                        html += '</td>';
                        html += '<td class="text-center"><span class="badge badge-primary">' + product.totalSold + '</span></td>';
                        html += '<td class="text-right text-success font-weight-bold">' + dashboardApp.formatCurrency(product.totalRevenue) + '</td>';
                        html += '</tr>';
                    });
                }
                
                $('#topProductsTable').html(html);
            },
            
            // Load recent orders
            loadRecentOrders: function() {
                _dashboardService.getRecentOrders(10)
                    .done(function(result) {
                        dashboardApp.renderRecentOrders(result);
                    })
                    .fail(function(error) {
                        console.error('Error loading recent orders:', error);
                        $('#recentOrdersTable').html(
                            '<tr><td colspan="4" class="text-center text-danger">Không thể tải dữ liệu</td></tr>'
                        );
                    });
            },
            
            // Render recent orders table
            renderRecentOrders: function(orders) {
                var html = '';
                
                if (orders.length === 0) {
                    html = '<tr><td colspan="4" class="text-center text-muted">Chưa có đơn hàng</td></tr>';
                } else {
                    orders.forEach(function(order) {
                        var statusClass = dashboardApp.getStatusClass(order.status);
                        
                        html += '<tr>';
                        html += '<td><a href="/Orders/Detail?id=' + order.orderId + '">' + order.orderCode + '</a></td>';
                        html += '<td>' + order.customerName + '</td>';
                        html += '<td class="text-right font-weight-bold">' + dashboardApp.formatCurrency(order.totalPrice) + '</td>';
                        html += '<td class="text-center">';
                        html += '<span class="badge ' + statusClass + ' status-badge">' + order.statusText + '</span>';
                        html += '</td>';
                        html += '</tr>';
                    });
                }
                
                $('#recentOrdersTable').html(html);
            },
            
            // Get status badge class
            getStatusClass: function(status) {
                switch(status) {
                    case 0: return 'badge-warning';  // Pending
                    case 1: return 'badge-info';     // Confirmed
                    case 2: return 'badge-success';  // Completed
                    case 3: return 'badge-danger';   // Cancelled
                    case 4: return 'badge-secondary'; // Returned
                    default: return 'badge-secondary';
                }
            },
            
            // Format currency
            formatCurrency: function(amount) {
                return new Intl.NumberFormat('vi-VN', {
                    style: 'currency',
                    currency: 'VND'
                }).format(amount);
            },
            
            // Initialize dashboard
            init: function() {
                dashboardApp.loadStats();
                dashboardApp.loadSalesChart();
                dashboardApp.loadCategoryChart();
                dashboardApp.loadTopProducts();
                dashboardApp.loadRecentOrders();
                
                // Auto refresh every 5 minutes
                setInterval(function() {
                    dashboardApp.loadStats();
                    dashboardApp.loadRecentOrders();
                }, 300000);
            }
        };
        
        // Start the app
        dashboardApp.init();
    });
})();
