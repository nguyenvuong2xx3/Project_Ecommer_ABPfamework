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
            
            // Render sales chart with ApexCharts (Beautiful Column Chart with Gradient)
            renderSalesChart: function(data) {
                if (dashboardApp.charts.salesChart) {
                    dashboardApp.charts.salesChart.destroy();
                }
                
                var options = {
                    series: [{
                        name: 'Doanh thu',
                        data: data.map(d => d.value)
                    }],
                    chart: {
                        type: 'bar',
                        height: 300,
                        toolbar: {
                            show: false
                        },
                        animations: {
                            enabled: true,
                            easing: 'easeinout',
                            speed: 800
                        }
                    },
                    plotOptions: {
                        bar: {
                            borderRadius: 8,
                            columnWidth: '60%',
                            distributed: false,
                            dataLabels: {
                                position: 'top'
                            }
                        }
                    },
                    dataLabels: {
                        enabled: false
                    },
                    colors: ['#3b82f6'],
                    fill: {
                        type: 'gradient',
                        gradient: {
                            shade: 'light',
                            type: 'vertical',
                            shadeIntensity: 0.5,
                            gradientToColors: ['#60a5fa'],
                            opacityFrom: 0.9,
                            opacityTo: 0.7,
                            stops: [0, 100]
                        }
                    },
                    stroke: {
                        show: true,
                        width: 2,
                        colors: ['transparent']
                    },
                    xaxis: {
                        categories: data.map(d => d.label),
                        labels: {
                            style: {
                                fontSize: '11px',
                                colors: '#6b7280'
                            }
                        },
                        axisBorder: {
                            show: false
                        },
                        axisTicks: {
                            show: false
                        }
                    },
                    yaxis: {
                        labels: {
                            style: {
                                fontSize: '11px',
                                colors: '#6b7280'
                            },
                            formatter: function(value) {
                                return dashboardApp.formatCurrencyShort(value);
                            }
                        }
                    },
                    grid: {
                        borderColor: '#f1f5f9',
                        strokeDashArray: 3,
                        xaxis: {
                            lines: {
                                show: false
                            }
                        },
                        yaxis: {
                            lines: {
                                show: true
                            }
                        }
                    },
                    tooltip: {
                        theme: 'light',
                        y: {
                            formatter: function(value) {
                                return dashboardApp.formatCurrency(value);
                            }
                        },
                        style: {
                            fontSize: '12px'
                        }
                    },
                    legend: {
                        show: false
                    }
                };
                
                dashboardApp.charts.salesChart = new ApexCharts(
                    document.querySelector("#salesChart"), 
                    options
                );
                dashboardApp.charts.salesChart.render();
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
            
            // Render category chart with ApexCharts (Beautiful Donut Chart)
            renderCategoryChart: function(data) {
                if (dashboardApp.charts.categoryChart) {
                    dashboardApp.charts.categoryChart.destroy();
                }
                
                var options = {
                    series: data.map(d => d.value),
                    chart: {
                        type: 'donut',
                        height: 300,
                        animations: {
                            enabled: true,
                            easing: 'easeinout',
                            speed: 800
                        }
                    },
                    labels: data.map(d => d.label),
                    colors: data.map(d => d.color),
                    plotOptions: {
                        pie: {
                            donut: {
                                size: '65%',
                                labels: {
                                    show: true,
                                    name: {
                                        show: true,
                                        fontSize: '14px',
                                        fontWeight: 600,
                                        color: '#374151'
                                    },
                                    value: {
                                        show: true,
                                        fontSize: '20px',
                                        fontWeight: 700,
                                        color: '#111827',
                                        formatter: function(val) {
                                            return dashboardApp.formatCurrencyShort(val);
                                        }
                                    },
                                    total: {
                                        show: true,
                                        label: 'Tổng doanh thu',
                                        fontSize: '12px',
                                        color: '#6b7280',
                                        formatter: function(w) {
                                            var total = w.globals.seriesTotals.reduce((a, b) => a + b, 0);
                                            return dashboardApp.formatCurrencyShort(total);
                                        }
                                    }
                                }
                            }
                        }
                    },
                    dataLabels: {
                        enabled: false
                    },
                    legend: {
                        position: 'bottom',
                        horizontalAlign: 'center',
                        fontSize: '11px',
                        fontWeight: 500,
                        markers: {
                            width: 10,
                            height: 10,
                            radius: 2
                        },
                        itemMargin: {
                            horizontal: 8,
                            vertical: 4
                        }
                    },
                    tooltip: {
                        theme: 'light',
                        y: {
                            formatter: function(value) {
                                return dashboardApp.formatCurrency(value);
                            }
                        },
                        style: {
                            fontSize: '12px'
                        }
                    },
                    stroke: {
                        show: true,
                        width: 2,
                        colors: ['#fff']
                    }
                };
                
                dashboardApp.charts.categoryChart = new ApexCharts(
                    document.querySelector("#categoryChart"), 
                    options
                );
                dashboardApp.charts.categoryChart.render();
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
            
            // Format currency short (for charts)
            formatCurrencyShort: function(amount) {
                if (amount >= 1000000000) {
                    return (amount / 1000000000).toFixed(1) + ' tỷ';
                } else if (amount >= 1000000) {
                    return (amount / 1000000).toFixed(1) + ' tr';
                } else if (amount >= 1000) {
                    return (amount / 1000).toFixed(0) + 'k';
                }
                return amount.toLocaleString('vi-VN');
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
