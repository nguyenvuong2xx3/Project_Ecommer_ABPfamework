(function () {
	$(function () {
		var _dashboardService = abp.services.app.dashboard;

		var dashboardApp = {
			charts: {
				salesChart: null,
				categoryChart: null
			},

			// Load dashboard stats
			loadStats: function () {
				abp.ui.setBusy();
				_dashboardService.getDashboardStats()
					.done(function (result) {
						dashboardApp.updateStats(result);
					})
					.fail(function (error) {
						abp.notify.error('Không thể tải thống kê dashboard');
						console.error(error);
					})
					.always(function () {
						abp.ui.clearBusy();
					});
			},

			// Update stats cards
			updateStats: function (data) {
				// Main stats
				$('#totalRevenue').html(dashboardApp.formatCurrency(data.totalRevenue));
				$('#totalOrders').text(data.totalOrders.toLocaleString('vi-VN'));
				$('#totalProducts').text(data.totalProducts.toLocaleString('vi-VN'));
				$('#totalCustomers').text(data.totalCustomers.toLocaleString('vi-VN'));

				// Growth indicators
				var revenueGrowth = data.revenueGrowth.toFixed(1);
				var revenueIcon = data.revenueGrowth >= 0 ? 'fa-arrow-up' : 'fa-arrow-down';
				var growthClass = data.revenueGrowth >= 0 ? 'positive' : 'negative';
				$('#revenueGrowth').attr('class', 'stats-growth ' + growthClass);
				$('#revenueGrowth').html(
					'<i class="fas ' + revenueIcon + '"></i> ' +
					Math.abs(revenueGrowth) + '% so với tháng trước'
				);

				var ordersGrowth = data.ordersGrowth.toFixed(1);
				var ordersIcon = data.ordersGrowth >= 0 ? 'fa-arrow-up' : 'fa-arrow-down';
				var ordersGrowthClass = data.ordersGrowth >= 0 ? 'positive' : 'negative';
				$('#ordersGrowth').attr('class', 'stats-growth ' + ordersGrowthClass);
				$('#ordersGrowth').html(
					'<i class="fas ' + ordersIcon + '"></i> ' +
					Math.abs(ordersGrowth) + '% so với tháng trước'
				);

				// Low stock info
				if (data.lowStockProducts > 0) {
					$('#lowStockInfo').html(
						'<i class="fas fa-exclamation-circle text-warning"></i> ' +
						data.lowStockProducts + ' sản phẩm sắp hết'
					);
				} else {
					$('#lowStockInfo').html('<i class="fas fa-check-circle text-success"></i> Tồn kho ổn định');
				}

				// Average order value
				$('#avgOrderValue').html(
					'<i class="fas fa-chart-line mr-1"></i>Giá trị TB: ' +
					dashboardApp.formatCurrencyShort(data.averageOrderValue)
				);

				// Additional stats
				$('#pendingOrders').text(data.pendingOrders);
				$('#completedOrders').text(data.completedOrders);
				$('#activeVouchers').text(data.activeVouchers);
				$('#lowStockProducts').text(data.lowStockProducts);

				// Add clickable class to low stock card if there are low stock products
				var $lowStockCard = $('.modern-mini-stat.stat-danger');
				if (data.lowStockProducts > 0) {
					$lowStockCard.addClass('clickable').css('cursor', 'pointer');
				} else {
					$lowStockCard.removeClass('clickable').css('cursor', 'default');
				}
			},

			// Load sales chart
			loadSalesChart: function () {
				_dashboardService.getSalesChartData(30)
					.done(function (result) {
						dashboardApp.renderSalesChart(result);
					})
					.fail(function (error) {
						console.error('Error loading sales chart:', error);
					});
			},

			// Render sales chart with ApexCharts (Beautiful Area Chart with Gradient)
			renderSalesChart: function (data) {
				if (dashboardApp.charts.salesChart) {
					dashboardApp.charts.salesChart.destroy();
				}

				var options = {
					series: [{
						name: 'Doanh thu',
						data: data.map(d => d.value)
					}],
					chart: {
						type: 'area',
						height: 280,
						toolbar: {
							show: false
						},
						animations: {
							enabled: true,
							easing: 'easeinout',
							speed: 1000
						},
						zoom: {
							enabled: false
						}
					},
					dataLabels: {
						enabled: false
					},
					stroke: {
						curve: 'smooth',
						width: 2.5
					},
					colors: ['#667eea'],
					fill: {
						type: 'gradient',
						gradient: {
							shade: 'light',
							type: 'vertical',
							shadeIntensity: 0.4,
							gradientToColors: ['#764ba2'],
							opacityFrom: 0.65,
							opacityTo: 0.15,
							stops: [0, 100]
						}
					},
					xaxis: {
						categories: data.map(d => d.label),
						labels: {
							style: {
								fontSize: '11px',
								colors: '#64748b',
								fontWeight: 500
							},
							rotate: -45,
							rotateAlways: false
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
								colors: '#64748b',
								fontWeight: 500
							},
							formatter: function (value) {
								return dashboardApp.formatCurrencyShort(value);
							}
						}
					},
					grid: {
						borderColor: '#e2e8f0',
						strokeDashArray: 4,
						xaxis: {
							lines: {
								show: false
							}
						},
						yaxis: {
							lines: {
								show: true
							}
						},
						padding: {
							top: 0,
							right: 0,
							bottom: 0,
							left: 10
						}
					},
					tooltip: {
						theme: 'light',
						y: {
							formatter: function (value) {
								return dashboardApp.formatCurrency(value);
							}
						},
						style: {
							fontSize: '12px'
						},
						marker: {
							show: true
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
			loadCategoryChart: function () {
				_dashboardService.getRevenueByCategory()
					.done(function (result) {
						dashboardApp.renderCategoryChart(result);
					})
					.fail(function (error) {
						console.error('Error loading category chart:', error);
					});
			},

			// Render category chart with ApexCharts (Beautiful Donut Chart)
			renderCategoryChart: function (data) {
				if (dashboardApp.charts.categoryChart) {
					dashboardApp.charts.categoryChart.destroy();
				}

				var options = {
					series: data.map(d => d.value),
					chart: {
						type: 'donut',
						height: 280,
						animations: {
							enabled: true,
							easing: 'easeinout',
							speed: 1000
						}
					},
					labels: data.map(d => d.label),
					colors: data.map(d => d.color),
					plotOptions: {
						pie: {
							donut: {
								size: '68%',
								labels: {
									show: true,
									name: {
										show: true,
										fontSize: '14px',
										fontWeight: 700,
										color: '#1e293b',
										offsetY: -10
									},
									value: {
										show: true,
										fontSize: '20px',
										fontWeight: 800,
										color: '#1e293b',
										offsetY: 5,
										formatter: function (val) {
											return dashboardApp.formatCurrencyShort(val);
										}
									},
									total: {
										show: true,
										showAlways: true,
										label: 'Tổng DT',
										fontSize: '13px',
										fontWeight: 600,
										color: '#64748b',
										formatter: function (w) {
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
						fontSize: '12px',
						fontWeight: 600,
						markers: {
							width: 10,
							height: 10,
							radius: 3
						},
						itemMargin: {
							horizontal: 10,
							vertical: 5
						}
					},
					tooltip: {
						theme: 'light',
						y: {
							formatter: function (value) {
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
			loadTopProducts: function () {
				_dashboardService.getTopSellingProducts(5)
					.done(function (result) {
						dashboardApp.renderTopProducts(result);
					})
					.fail(function (error) {
						console.error('Error loading top products:', error);
						$('#topProductsTable').html(
							'<tr><td colspan="3" class="text-center text-danger py-3">Không thể tải dữ liệu</td></tr>'
						);
					});
			},

			// Render top products table
			renderTopProducts: function (products) {
				var html = '';

				if (products.length === 0) {
					html = '<tr><td colspan="3" class="text-center text-muted py-3">Chưa có dữ liệu</td></tr>';
				} else {
					products.forEach(function (product, index) {
						html += '<tr>';
						html += '<td>';
						html += '<div class="d-flex align-items-center">';
						html += '<img src="' + product.imageUrl + '" class="product-thumb mr-2">';
						html += '<div>';
						html += '<div class="font-weight-bold mb-1" style="font-size: 0.85rem;">' + product.productName + '</div>';
						html += '<small class="text-muted"><i class="fas fa-trophy text-warning mr-1"></i>Top ' + (index + 1) + '</small>';
						html += '</div>';
						html += '</div>';
						html += '</td>';
						html += '<td class="text-center">';
						html += '<span class="modern-badge badge-primary">';
						html += '<i class="fas fa-box mr-1"></i>' + product.totalSold;
						html += '</span>';
						html += '</td>';
						html += '<td class="text-right">';
						html += '<strong class="text-success">' + dashboardApp.formatCurrency(product.totalRevenue) + '</strong>';
						html += '</td>';
						html += '</tr>';
					});
				}

				$('#topProductsTable').html(html);
			},

			// Load recent orders
			loadRecentOrders: function () {
				_dashboardService.getRecentOrders(10)
					.done(function (result) {
						dashboardApp.renderRecentOrders(result);
					})
					.fail(function (error) {
						console.error('Error loading recent orders:', error);
						$('#recentOrdersTable').html(
							'<tr><td colspan="4" class="text-center text-danger py-3">Không thể tải dữ liệu</td></tr>'
						);
					});
			},

			// Render recent orders table
			renderRecentOrders: function (orders) {
				var html = '';

				if (orders.length === 0) {
					html = '<tr><td colspan="4" class="text-center text-muted py-3">Chưa có đơn hàng</td></tr>';
				} else {
					orders.forEach(function (order) {
						var statusBadge = dashboardApp.getStatusBadge(order.status, order.statusText);

						html += '<tr>';
						html += '<td><a href="/Orders/Detail?id=' + order.orderId + '" class="font-weight-bold text-primary">#' + order.orderCode + '</a></td>';
						html += '<td>' + order.customerName + '</td>';
						html += '<td class="text-right"><strong>' + dashboardApp.formatCurrency(order.totalPrice) + '</strong></td>';
						html += '<td class="text-center">' + statusBadge + '</td>';
						html += '</tr>';
					});
				}

				$('#recentOrdersTable').html(html);
			},

			// Get status badge with modern design
			getStatusBadge: function (status, statusText) {
				var badgeClass = '';
				var icon = '';

				switch (status) {
					case 0:
						badgeClass = 'badge-warning';
						icon = 'fa-clock';
						break;
					case 1:
						badgeClass = 'badge-primary';
						icon = 'fa-check';
						break;
					case 2:
						badgeClass = 'badge-success';
						icon = 'fa-check-circle';
						break;
					case 3:
						badgeClass = 'badge-danger';
						icon = 'fa-times-circle';
						break;
					default:
						badgeClass = 'badge-secondary';
						icon = 'fa-info-circle';
				}

				return '<span class="modern-badge ' + badgeClass + '"><i class="fas ' + icon + '"></i>' + statusText + '</span>';
			},

			// Format currency
			formatCurrency: function (amount) {
				return new Intl.NumberFormat('vi-VN', {
					style: 'currency',
					currency: 'VND'
				}).format(amount);
			},

			// Format currency short (for charts)
			formatCurrencyShort: function (amount) {
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
			init: function () {
				var _lowProductVariantModal = new app.ModalManager({
					viewUrl: abp.appPath + 'Home/LowProductVariantModal',
					scriptUrl: abp.appPath + 'view-resources/Views/Home/_LowProductVariantModal.js',
					modalClass: 'LowProductVariantModal',
					modalSize: 'modal-lg'
				});

				// Load all dashboard data
				dashboardApp.loadStats();
				dashboardApp.loadSalesChart();
				dashboardApp.loadCategoryChart();
				dashboardApp.loadTopProducts();
				dashboardApp.loadRecentOrders();

				// Add click event for low stock card
				$(document).on('click', '.modern-mini-stat.stat-danger.clickable', function () {
					_lowProductVariantModal.open();
				});

				// Auto refresh every 5 minutes
				setInterval(function () {
					dashboardApp.loadStats();
					dashboardApp.loadRecentOrders();
				}, 300000);
			}
		};

		// Start the app
		dashboardApp.init();
	});
})();