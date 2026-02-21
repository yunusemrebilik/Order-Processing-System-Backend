using System.Security.Claims;
using ECommerce.Application.Cart.AddToCart;
using ECommerce.Application.Cart.Checkout;
using ECommerce.Application.Cart.GetCart;
using ECommerce.Application.Cart.RemoveCartItem;
using ECommerce.Application.Cart.UpdateCartItem;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User ID not found in token");

    /// <summary>
    /// Get the current user's shopping cart.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _mediator.Send(new GetCartQuery(GetUserId()));
        return Ok(cart);
    }

    /// <summary>
    /// Add a product to the cart. If already in cart, quantity is incremented.
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart(
        AddToCartCommand command,
        [FromServices] IValidator<AddToCartCommand> validator)
    {
        var commandWithUser = command with { UserId = GetUserId() };

        var validationResult = await validator.ValidateAsync(commandWithUser);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        await _mediator.Send(commandWithUser);
        return NoContent();
    }

    /// <summary>
    /// Update the quantity of a cart item. Set quantity to 0 to remove.
    /// </summary>
    [HttpPut("items/{productId}")]
    public async Task<IActionResult> UpdateCartItem(string productId, [FromBody] UpdateQuantityRequest request)
    {
        var command = new UpdateCartItemCommand
        {
            UserId = GetUserId(),
            ProductId = productId,
            Quantity = request.Quantity
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Remove a product from the cart.
    /// </summary>
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveCartItem(string productId)
    {
        var command = new RemoveCartItemCommand
        {
            UserId = GetUserId(),
            ProductId = productId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Checkout: convert cart to order and clear the cart.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest? request)
    {
        var command = new CheckoutCommand
        {
            UserId = GetUserId(),
            ShippingAddress = request?.ShippingAddress,
            Notes = request?.Notes
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCart), result);
    }
}

// Simple DTOs for request bodies
public record UpdateQuantityRequest(int Quantity);
public record CheckoutRequest(string? ShippingAddress, string? Notes);
